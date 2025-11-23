using System;
using JasperFx.CommandLine;
using JasperFx.Core.Reflection;
using Spectre.Console;

namespace Lamar.CommandLine
{
    [Description("Runs Lamar's type scanning diagnostics", Name = "lamar-scanning")]
    public class LamarScanningCommand : JasperFxCommand<NetCoreInput>
    {
        public override bool Execute(NetCoreInput input)
        {
            AnsiConsole.Write(new FigletText("Lamar"){Color = Color.Blue});

            
            using (var host = input.BuildHost())
            {
                var container = host.Services.As<IContainer>();

                var scanning = container.WhatDidIScan();
                Console.WriteLine(scanning);
            }

            return true;
        }
    }
}