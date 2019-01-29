using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Lamar;
using LamarCompiler;
using LamarCompiler.Model;
using LamarRest.Internal.Frames;
using Microsoft.Extensions.Configuration;

namespace LamarRest.Internal
{
    public class GeneratedServiceType
    {
        private readonly GeneratedType _generatedType;

        public static GeneratedServiceType For(Type serviceType, IContainer container)
        {
            var assembly = new GeneratedAssembly(new GenerationRules("LamarRest"));
            var generatedType = new GeneratedServiceType(assembly, serviceType);
            
            container.CompileWithInlineServices(assembly);

            return generatedType;
        }
        
        public GeneratedServiceType(GeneratedAssembly assembly, Type serviceType)
        {
            var name = serviceType.Name.StartsWith("I")
                ? serviceType.Name.TrimStart('I')
                : serviceType.Name + "Implementation";

            _generatedType = assembly.AddType(name, serviceType);

            foreach (var methodInfo in serviceType.GetMethods().Where(x => x.HasAttribute<PathAttribute>()))
            {
                var generatedMethod = _generatedType.MethodFor(methodInfo.Name);
                BuildOut(serviceType, methodInfo, generatedMethod);
            }
        }

        public Type CompiledType => _generatedType.CompiledType;
        public string SourceCode => _generatedType.SourceCode;

        public static void BuildOut(Type interfaceType, MethodInfo definition, GeneratedMethod generated)
        {
            var path = definition.GetCustomAttribute<PathAttribute>();

            generated.Frames.Add(new BuildClientFrame(interfaceType));

            var urlFrame = new FillUrlFrame(definition);
            generated.Frames.Add(urlFrame);


            var inputType = DetermineRequestType(definition);
            if (inputType == null)
            {
                generated.Frames.Add(new BuildRequestFrame(path.Method, urlFrame, null));
            }
            else
            {
                var serializeFrame = new SerializeJsonFrame(inputType);
                generated.Frames.Add(serializeFrame);
                generated.Frames.Add(new BuildRequestFrame(path.Method, urlFrame, serializeFrame));
            }

            generated.Frames.Call<HttpClient>(x => x.SendAsync(null));

            var returnType = DetermineResponseType(definition);
            if (returnType != null)
            {
                var deserialize = new DeserializeObjectFrame(returnType);
                generated.Frames.Add(deserialize);
                generated.Frames.Return(deserialize.ReturnValue);
            }
        }

        public static Type DetermineResponseType(MethodInfo definition)
        {
            if (definition.ReturnType == typeof(void)) return null;
            if (definition.ReturnType == typeof(Task)) return null;

            if (definition.ReturnType.Closes(typeof(Task<>)))
            {
                return definition.ReturnType.GetGenericArguments().Single();
            }

            return null;
        }

        public static Type DetermineRequestType(MethodInfo definition)
        {
            var parameters = definition.GetParameters();
            var path = definition.GetAttribute<PathAttribute>();
            var segments = path.Path.TrimStart('/').Split('/');

            var first = definition.GetParameters().FirstOrDefault();
            if (first == null) return null;

            var segmentName = $"{{{first.Name}}}";
            if (segments.Contains(segmentName)) return null;

            return first.ParameterType;
        }
    }
}