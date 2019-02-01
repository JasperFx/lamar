using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCompiler.Model;
using LamarCompiler.Util;

namespace LamarCompiler
{
    public class GeneratedAssembly
    {
        public readonly List<GeneratedType> GeneratedTypes = new List<GeneratedType>();

        public GeneratedAssembly(GenerationRules generation)
        {
            Generation = generation;
        }

        public GenerationRules Generation { get; }
        
        private readonly IList<Assembly> _assemblies = new List<Assembly>();

        public void ReferenceAssembly(Assembly assembly)
        {
            _assemblies.Fill(assembly);
        }
        
        public GeneratedType AddType(string typeName, Type baseType)
        {
            // TODO -- assert that it's been generated already?

            var generatedType = new GeneratedType(Generation, typeName);
            if (baseType.IsInterface)
            {
                generatedType.Implements(baseType);
            }
            else
            {
                generatedType.InheritsFrom(baseType);
            }

            GeneratedTypes.Add(generatedType);

            return generatedType;
        }

        public void CompileAll(IServiceVariableSource services = null)
        {
            var code = GenerateCode(services);

            var generator = buildGenerator(Generation);

            var assembly = generator.Generate(code);

            var generated = assembly.GetExportedTypes().ToArray();

            foreach (var generatedType in GeneratedTypes)
            {
                generatedType.FindType(generated);
            }
        }

        public string GenerateCode(IServiceVariableSource services = null)
        {
            foreach (var generatedType in GeneratedTypes)
            {
                services?.StartNewType();
                generatedType.ArrangeFrames(services);
            }

            var namespaces = GeneratedTypes
                .SelectMany(x => x.AllInjectedFields)
                .Select(x => x.ArgType.Namespace)
                .Concat(new string[]{typeof(Task).Namespace})
                .Distinct().ToList();

            var writer = new SourceWriter();

            foreach (var ns in namespaces.OrderBy(x => x))
            {
                writer.Write($"using {ns};");
            }

            writer.BlankLine();

            writer.Namespace(Generation.ApplicationNamespace);

            foreach (var @class in GeneratedTypes)
            {
                writer.WriteLine($"// START: {@class.TypeName}");
                @class.Write(writer);
                writer.WriteLine($"// END: {@class.TypeName}");

                writer.WriteLine("");
                writer.WriteLine("");
            }

            writer.FinishBlock();


            var code = writer.Code();

            attachSourceCodeToChains(code);


            return code;
        }

        private AssemblyGenerator buildGenerator(GenerationRules generation)
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(GetType().Assembly);
            generator.ReferenceAssembly(typeof(Task).Assembly);

            foreach (var assembly in generation.Assemblies)
            {
                generator.ReferenceAssembly(assembly);
            }

            foreach (var assembly in _assemblies)
            {
                generator.ReferenceAssembly(assembly);
            }

            var assemblies = GeneratedTypes
                .SelectMany(x => x.AssemblyReferences())
                .Distinct().ToArray();

            assemblies
                .Each(x => generator.ReferenceAssembly(x));

            return generator;
        }

        private void attachSourceCodeToChains(string code)
        {
            var parser = new SourceCodeParser(code);
            foreach (var type in GeneratedTypes)
            {
                type.SourceCode = parser.CodeFor(type.TypeName);
            }
        }

        /// <summary>
        /// Creates a new GeneratedAssembly with default generation
        /// rules and using the namespace "LamarGenerated"
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static GeneratedAssembly Empty()
        {
            return new GeneratedAssembly(new GenerationRules("LamarGenerated"));
        }
    }
}
