using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration
{
    public class DynamicCodeBuilder
    {
        public DynamicCodeBuilder(GenerationRules rules, IServiceProvider services, IGeneratesCode[] generators)
        {
            Rules = rules;
            Services = services;
            Generators = generators;
        }

        public GenerationRules Rules { get; }

        public IServiceVariableSource ServiceVariableSource { get; set; }

        public string GenerateAllCode()
        {
            var writer = new StringWriter();

            foreach (var generator in Generators)
            {
                var code = generateCode(generator);
                writer.WriteLine(code);
                writer.WriteLine();
                writer.WriteLine();
            }

            return writer.ToString();
        }

        public string DeleteAllGeneratedCode()
        {
            var directory = Rules.GeneratedCodeOutputPath.ToFullPath();
            var fileSystem = new FileSystem();
            fileSystem.CleanDirectory(directory);
            fileSystem.DeleteDirectory(directory);

            return directory;
        }

        public string GenerateCodeFor(string childNamespace)
        {
            var generator = Generators.FirstOrDefault(x => x.ChildNamespace.EqualsIgnoreCase(childNamespace));
            if (generator == null)
            {
                throw new ArgumentOutOfRangeException($"Unknown {nameof(childNamespace)} '{childNamespace}'. Known code types are {ChildNamespaces.Join(", ")}");
            }

            return generateCode(generator);
        }

        public void WriteGeneratedCode(Action<string> onFileWritten, string directory = null)
        {
            directory = directory ?? Rules.GeneratedCodeOutputPath.ToFullPath();
            
            
            new FileSystem().CreateDirectory(directory);


            foreach (var generator in Generators)
            {
                var exportDirectory = generator.ToExportDirectory(directory);
                
                
                foreach (var file in generator.BuildFiles())
                {
                    var generatedAssembly = generator.StartAssembly(Rules);
                    file.AssembleTypes(generatedAssembly);

                    var code = generatedAssembly.GenerateCode(ServiceVariableSource);
                    var fileName = Path.Combine(exportDirectory, file.FileName.Replace(" ", "_") + ".cs");
                    File.WriteAllText(fileName, code);
                    onFileWritten(fileName);
                }

            }
        }

        private string generateCode(IGeneratesCode generator)
        {
            if (generator.ChildNamespace.IsEmpty())
            {
                throw new InvalidOperationException($"Missing {nameof(IGeneratesCode.ChildNamespace)} for {generator}");
            }

            var @namespace = generator.ToNamespace(Rules);
            
            var generatedAssembly = new GeneratedAssembly(Rules, @namespace);
            var files = generator.BuildFiles();
            foreach (var file in files)
            {
                file.AssembleTypes(generatedAssembly);
            }

            return generatedAssembly.GenerateCode(ServiceVariableSource);
        }

        /// <summary>
        /// Attempts to generate all the known code types in the system
        /// </summary>
        /// <param name="withAssembly"></param>
        /// <exception cref="GeneratorCompilationFailureException"></exception>
        public void TryBuildAndCompileAll(Action<GeneratedAssembly, IServiceVariableSource> withAssembly)
        {
            foreach (var generator in Generators)
            {
                var generatedAssembly = generator.AssembleTypes(Rules);

                try
                {
                    withAssembly(generatedAssembly, ServiceVariableSource);
                }
                catch (Exception e)
                {
                    throw new GeneratorCompilationFailureException(generator, e);
                }
            }
        }

        /// <summary>
        /// Attach pre-built types in the application assembly
        /// </summary>
        /// <param name="assembly">The assembly containing the pre-built types. If null, this falls back to the entry assembly of the running application</param>
        public async Task LoadPrebuiltTypes(Assembly assembly = null)
        {
            foreach (var generator in Generators)
            {
                foreach (var file in generator.BuildFiles())
                {
                    var @namespace = $"{Rules.ApplicationNamespace}.{generator.ChildNamespace}";
                    await file.AttachTypes(Rules, assembly ?? Rules.ApplicationAssembly, Services, @namespace);
                }
            }
        }

        public string[] ChildNamespaces => Generators.Select(x => x.ChildNamespace).ToArray();

        public IServiceProvider Services { get; }

        public IGeneratesCode[] Generators { get; }
    }
}