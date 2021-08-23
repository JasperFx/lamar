using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace GeneratorTarget
{
    public class ConsoleWriterGenerator : IGeneratesCode
    {
        private GeneratedType _generatedType;
        private IConsoleWriter _writer;
        public string TypeName { get; }
        public string Message { get; }

        public ConsoleWriterGenerator(string typeName, string message, string codeType)
        {
            TypeName = typeName;
            Message = message;
            CodeType = codeType;
        }

        public IServiceVariableSource AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly)
        {
            _generatedType = assembly.AddType(TypeName, typeof(IConsoleWriter));

            var generatedMethod = _generatedType.MethodFor(nameof(IConsoleWriter.Write));
            var consoleMethod = typeof(Console).GetMethods().Single(x => x.Name == nameof(Console.WriteLine) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string));
            
            var @call = new MethodCall(typeof(Console),consoleMethod);
            @call.Arguments[0] = new Value(Message);

            generatedMethod.Frames.Add(@call);

            return null;
        }

        public Task AttachPreBuiltTypes(GenerationRules rules, Assembly assembly, IServiceProvider services)
        {
            var type = assembly.GetExportedTypes().FirstOrDefault(x => x.Name == TypeName);
            if (type == null)
            {
                throw new InvalidOperationException("I could not find a type with name " + TypeName);
            }

            _writer = Activator.CreateInstance(type).As<IConsoleWriter>();

            return Task.CompletedTask;
        }

        public Task AttachGeneratedTypes(GenerationRules rules, IServiceProvider services)
        {
            _writer = services.As<IContainer>().GetInstance(_generatedType.CompiledType).As<IConsoleWriter>();
            return Task.CompletedTask;
        }

        public string CodeType { get; }

        public void WriteToConsole()
        {
            _writer?.Write();
        }
    }
}