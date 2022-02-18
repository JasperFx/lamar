using System;
using System.IO;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

#nullable enable

namespace LamarCompiler
{
    public static class CodeFileExtensions
    {
        public static async Task Initialize(this ICodeFile file, GenerationRules rules, ICodeFileCollection parent, IServiceProvider? services)
        {
            var @namespace = parent.ToNamespace(rules);
            
            if (rules.TypeLoadMode == TypeLoadMode.Dynamic)
            {
                Console.WriteLine($"Generated code for {parent.ChildNamespace}.{file.FileName}");
                
                var generatedAssembly = parent.StartAssembly(rules);
                file.AssembleTypes(generatedAssembly);
                var serviceVariables = services?.GetService(typeof(IServiceVariableSource)) as IServiceVariableSource;
                        
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
                var serviceVariables = services?.GetService(typeof(IServiceVariableSource)) as IServiceVariableSource;
                
                
                var compiler = new AssemblyGenerator();
                compiler.Compile(generatedAssembly, serviceVariables);
                
                await file.AttachTypes(rules, generatedAssembly.Assembly, services, @namespace);

                if (rules.SourceCodeWritingEnabled)
                {
                    var code = compiler.Code;
                    file.WriteCodeFile(parent, rules, code);
                }
                
                Console.WriteLine($"Generated and compiled code in memory for {parent.ChildNamespace}.{file.FileName}");
            }
            

        }
        
        /// <summary>
        /// Initialize dynamic code compilation by either loading the expected type
        /// from the supplied assembly or dynamically generating and compiling the code
        /// on demand
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rules"></param>
        /// <param name="parent"></param>
        /// <param name="services"></param>
        /// <exception cref="ExpectedTypeMissingException"></exception>
        public static void InitializeSynchronously(this ICodeFile file, GenerationRules rules, ICodeFileCollection parent, IServiceProvider? services)
        {
            var @namespace = parent.ToNamespace(rules);
            
            if (rules.TypeLoadMode == TypeLoadMode.Dynamic)
            {
                Console.WriteLine($"Generated code for {parent.ChildNamespace}.{file.FileName}");
                
                var generatedAssembly = parent.StartAssembly(rules);
                file.AssembleTypes(generatedAssembly);
                var serviceVariables = services?.GetService(typeof(IServiceVariableSource)) as IServiceVariableSource;
                        
                var compiler = new AssemblyGenerator();
                compiler.Compile(generatedAssembly, serviceVariables);
                file.AttachTypesSynchronously(rules, generatedAssembly.Assembly, services, @namespace);

                return;
            }
            
            var found = file.AttachTypesSynchronously(rules, rules.ApplicationAssembly, services, @namespace);
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
                var serviceVariables = services?.GetService(typeof(IServiceVariableSource)) as IServiceVariableSource;
                
                
                var compiler = new AssemblyGenerator();
                compiler.Compile(generatedAssembly, serviceVariables);
                
                file.AttachTypesSynchronously(rules, generatedAssembly.Assembly, services, @namespace);

                if (rules.SourceCodeWritingEnabled)
                {
                    var code = compiler.Code;
                    file.WriteCodeFile(parent, rules, code);
                }
                
                Console.WriteLine($"Generated and compiled code in memory for {parent.ChildNamespace}.{file.FileName}");
            }
            

        }

        public static void WriteCodeFile(this ICodeFile file, ICodeFileCollection parent, GenerationRules rules, string code)
        {
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
        }
    }
}