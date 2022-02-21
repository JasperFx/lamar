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
        public DynamicCodeBuilder(IServiceProvider services, ICodeFileCollection[] collections)
        {
            Services = services;
            Collections = collections;
        }

        public IServiceVariableSource ServiceVariableSource { get; set; }

        public string GenerateAllCode()
        {
            var writer = new StringWriter();

            foreach (var generator in Collections)
            {
                var code = generateCode(generator);
                writer.WriteLine(code);
                writer.WriteLine();
                writer.WriteLine();
            }

            return writer.ToString();
        }

        public void DeleteAllGeneratedCode()
        {
            var fileSystem = new FileSystem();
            foreach (var directory in Collections.Select(x => x.Rules.GeneratedCodeOutputPath).Distinct())
            {
                fileSystem.CleanDirectory(directory);
                fileSystem.DeleteDirectory(directory);
                
                Console.WriteLine($"Deleted directory {directory}");
            }
        }

        public string GenerateCodeFor(string childNamespace)
        {
            var generator = Collections.FirstOrDefault(x => x.ChildNamespace.EqualsIgnoreCase(childNamespace));
            if (generator == null)
            {
                throw new ArgumentOutOfRangeException($"Unknown {nameof(childNamespace)} '{childNamespace}'. Known code types are {ChildNamespaces.Join(", ")}");
            }

            return generateCode(generator);
        }

        public void WriteGeneratedCode(Action<string> onFileWritten, string directory = null)
        {
            var fileSystem = new FileSystem();

            foreach (var collection in Collections)
            {
                directory = directory ?? collection.Rules.GeneratedCodeOutputPath.ToFullPath();
                fileSystem.CreateDirectory(directory);
                
                var exportDirectory = collection.ToExportDirectory(directory);
                
                
                foreach (var file in collection.BuildFiles())
                {
                    var generatedAssembly = collection.StartAssembly(collection.Rules);
                    file.AssembleTypes(generatedAssembly);

                    var code = generatedAssembly.GenerateCode(ServiceVariableSource);
                    var fileName = Path.Combine(exportDirectory, file.FileName.Replace(" ", "_") + ".cs");
                    File.WriteAllText(fileName, code);
                    onFileWritten(fileName);
                }

            }
        }

        private string generateCode(ICodeFileCollection collection)
        {
            if (collection.ChildNamespace.IsEmpty())
            {
                throw new InvalidOperationException($"Missing {nameof(ICodeFileCollection.ChildNamespace)} for {collection}");
            }

            var @namespace = collection.ToNamespace(collection.Rules);
            
            var generatedAssembly = new GeneratedAssembly(collection.Rules, @namespace);
            var files = collection.BuildFiles();
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
            foreach (var collection in Collections)
            {
                var generatedAssembly = collection.AssembleTypes(collection.Rules);

                try
                {
                    withAssembly(generatedAssembly, ServiceVariableSource);
                }
                catch (Exception e)
                {
                    throw new GeneratorCompilationFailureException(collection, e);
                }
            }
        }

        /// <summary>
        /// Attach pre-built types in the application assembly
        /// </summary>
        /// <param name="assembly">The assembly containing the pre-built types. If null, this falls back to the entry assembly of the running application</param>
        public async Task LoadPrebuiltTypes(Assembly assembly = null)
        {
            foreach (var collection in Collections)
            {
                foreach (var file in collection.BuildFiles())
                {
                    var @namespace = $"{collection.Rules.ApplicationNamespace}.{collection.ChildNamespace}";
                    await file.AttachTypes(collection.Rules, assembly ?? collection.Rules.ApplicationAssembly, Services, @namespace);
                }
            }
        }

        public string[] ChildNamespaces => Collections.Select(x => x.ChildNamespace).ToArray();

        public IServiceProvider Services { get; }

        public ICodeFileCollection[] Collections { get; }
    }
}