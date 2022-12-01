using System;
using System.Linq;
using JasperFx.StringExtensions;
using LamarCodeGeneration.Model;
using LamarCompiler;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Extensions.Hosting;
#endif
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Spectre.Console;

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

            var collections = host.Services.GetServices<ICodeFileCollection>().ToArray();
            if (!collections.Any())
            {
                AnsiConsole.Write($"[red]No registered {nameof(ICodeFileCollection)} services were detected, aborting.[/]");
                return false;
            }

            var builder = new DynamicCodeBuilder(host.Services, collections)
            {
                ServiceVariableSource = host.Services.GetService<IServiceVariableSource>()
            };

            switch (input.Action)
            {
                case CodeAction.preview:
                    var code = input.TypeFlag.IsEmpty() ? builder.GenerateAllCode() : builder.GenerateCodeFor(input.TypeFlag);
                    Console.WriteLine(code);
                    break;
                    
                case CodeAction.test:
                    Console.WriteLine("Trying to generate all code and compile, this might take a bit.");
                    builder.TryBuildAndCompileAll((a, s) => new AssemblyGenerator().Compile(a, s));
                    AnsiConsole.Write("[green]Success![/]");
                    break;
                    
                case CodeAction.delete:
                    builder.DeleteAllGeneratedCode();
                    
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