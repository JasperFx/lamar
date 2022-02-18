using System;
using System.Linq;
using System.Reflection;
using Baseline;
using LamarCodeGeneration.Model;
using LamarCompiler;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Extensions.Hosting;
#endif
using Microsoft.Extensions.DependencyInjection;
using Oakton;

[assembly:OaktonCommandAssembly]

namespace LamarCodeGeneration.Commands
{
    [Description("Utilities for working with LamarCodeGeneration and LamarCompiler", Name = "codegen")]
    public class GenerateCodeCommand : OaktonCommand<GenerateCodeInput>
    {
        public GenerateCodeCommand()
        {
            Usage("Preview").Arguments();
            Usage("All actions").Arguments(x => x.Action);
        }

        public override bool Execute(GenerateCodeInput input)
        {
            using var host = input.BuildHost();
            var builder = host.Services.GetRequiredService<DynamicCodeBuilder>();
            builder.ServiceVariableSource = host.Services.GetService<IServiceVariableSource>();
            
            if (!builder.ChildNamespaces.Any())
            {
                Console.WriteLine($"No registered {nameof(ICodeFileCollection)} services registered, exiting");
                return true;
            }

            switch (input.Action)
            {
                case CodeAction.preview:
                    var code = input.TypeFlag.IsEmpty() ? builder.GenerateAllCode() : builder.GenerateCodeFor(input.TypeFlag);
                    Console.WriteLine(code);
                    break;
                    
                case CodeAction.test:
                    Console.WriteLine("Trying to generate all code and compile, this might take a bit.");
                    builder.TryBuildAndCompileAll((a, s) => new AssemblyGenerator().Compile(a, s));
                    ConsoleWriter.Write(ConsoleColor.Green, "Success!");
                    break;
                    
                case CodeAction.delete:
                    var directory = builder.DeleteAllGeneratedCode();
                    Console.WriteLine("Deleting all files in directory " + directory);
                    
                    break;
                    
                case CodeAction.write:
                    builder.WriteGeneratedCode(file => Console.WriteLine("Wrote generated code file to " + file));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }


            return true;
        }


    }
}