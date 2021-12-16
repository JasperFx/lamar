using System;
using System.IO;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCompiler
{
    public static class CodeFileExtensions
    {
        public static async Task Initialize(this ICodeFile file, GenerationRules rules, IGeneratesCode parent, IServiceProvider services)
        {
            var @namespace = parent.ToNamespace(rules);
            
            if (rules.TypeLoadMode == TypeLoadMode.Dynamic)
            {
                Console.WriteLine($"Generated code for {parent.ChildNamespace}.{file.FileName}");
                
                var generatedAssembly = parent.StartAssembly(rules);
                file.AssembleTypes(generatedAssembly);
                var serviceVariables = services.GetService(typeof(IServiceVariableSource)) as IServiceVariableSource;
                        
                var compiler = new AssemblyGenerator();
                compiler.Compile(generatedAssembly, serviceVariables);
                await file.AttachTypes(rules, generatedAssembly.Assembly, services, @namespace);

                return;
            }
            
            var found = await file.AttachTypes(rules, rules.ApplicationAssembly, services, @namespace);
            if (found)
            {
                Console.WriteLine($"Types from code file {parent.ChildNamespace}.{file.FileName} were loaded from assembly {rules.ApplicationAssembly.GetName()}");
            }
            
            if (!found)
            {
                if (rules.TypeLoadMode == TypeLoadMode.Static)
                {
                    throw new ExpectedTypeMissingException(
                        $"Could not load expected pre-built types for code file {file.FileName} ({file})");
                }
                
                var generatedAssembly = parent.StartAssembly(rules);
                file.AssembleTypes(generatedAssembly);
                var serviceVariables = services.GetService(typeof(IServiceVariableSource)) as IServiceVariableSource;
                var code = generatedAssembly.GenerateCode(serviceVariables);
                try
                {
                    var directory = parent.ToExportDirectory(rules.GeneratedCodeOutputPath);
                    var fileName = Path.Combine(directory, file.FileName.Replace(" ", "_") + ".cs");
                    File.WriteAllText(fileName, code);
                    Console.WriteLine("Generated code to " + fileName.ToFullPath());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to write code file");
                    Console.WriteLine(e.ToString());
                }        
                
                
                var compiler = new AssemblyGenerator();
                compiler.Compile(generatedAssembly, serviceVariables);
                await file.AttachTypes(rules, generatedAssembly.Assembly, services, @namespace);
                Console.WriteLine($"Generated and compiled code in memory for {parent.ChildNamespace}.{file.FileName}");
            }
            

        }
    }
}