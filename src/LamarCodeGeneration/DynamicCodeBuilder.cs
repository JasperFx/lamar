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
        private readonly GenerationRules _rules;
        private readonly IServiceProvider _services;
        private readonly IGeneratesCode[] _generators;

        public DynamicCodeBuilder(GenerationRules rules, IServiceProvider services, IGeneratesCode[] generators)
        {
            _rules = rules;
            _services = services;
            _generators = generators;
        }

        public IServiceVariableSource ServiceVariableSource { get; set; }

        public string GenerateAllCode()
        {
            var writer = new StringWriter();

            foreach (var generator in _generators)
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
            var directory = _rules.GeneratedCodeOutputPath.ToFullPath();
            var fileSystem = new FileSystem();
            fileSystem.CleanDirectory(directory);
            fileSystem.DeleteDirectory(directory);

            return directory;
        }

        public string GenerateCodeFor(string childNamespace)
        {
            var generator = _generators.FirstOrDefault(x => x.ChildNamespace.EqualsIgnoreCase(childNamespace));
            if (generator == null)
            {
                throw new ArgumentOutOfRangeException($"Unknown {nameof(childNamespace)} '{childNamespace}'. Known code types are {ChildNamespaces.Join(", ")}");
            }

            return generateCode(generator);
        }

        public void WriteGeneratedCode(Action<string> onFileWritten, string directory = null)
        {
            directory = directory ?? _rules.GeneratedCodeOutputPath.ToFullPath();
            
            
            new FileSystem().CreateDirectory(directory);


            foreach (var generator in _generators)
            {
                var exportDirectory = generator.ToExportDirectory(directory);
                new FileSystem().CreateDirectory(exportDirectory);
                
                foreach (var file in generator.BuildFiles())
                {
                    var generatedAssembly = generator.StartAssembly(_rules);
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

            var @namespace = $"{_rules.ApplicationNamespace}.{generator.ChildNamespace}";
            
            var generatedAssembly = new GeneratedAssembly(_rules, @namespace);
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
            foreach (var generator in _generators)
            {
                var generatedAssembly = generator.AssembleTypes(_rules);

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
            foreach (var generator in _generators)
            {
                foreach (var file in generator.BuildFiles())
                {
                    await file.AttachTypes(_rules, assembly ?? _rules.ApplicationAssembly, _services);
                }
            }
        }

        public string[] ChildNamespaces => _generators.Select(x => x.ChildNamespace).ToArray();

    }
}