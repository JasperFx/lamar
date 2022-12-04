﻿using System.Threading.Tasks;
using JasperFx.CodeGeneration;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;

[assembly:OaktonCommandAssembly]

namespace GeneratorTarget
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return Host.CreateDefaultBuilder()
                .UseLamar((c, services) =>
                {
                    services.AddSingleton<ICodeFileCollection>(new GreeterGenerator());
                    services.AddSingleton<ICodeFileCollection>(new GreeterGenerator2());
                })
                .RunOaktonCommands(args);

        }
    }
}