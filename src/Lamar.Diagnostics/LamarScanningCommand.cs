using System;
using LamarCodeGeneration.Util;
using Oakton;
using Spectre.Console;

namespace Lamar.Diagnostics
{
    [Description("Runs Lamar's type scanning diagnostics", Name = "lamar-scanning")]
    public class LamarScanningCommand : OaktonCommand<NetCoreInput>
    {
        public override bool Execute(NetCoreInput input)
        {
            AnsiConsole.Render(new FigletText("Lamar"){Color = Color.Blue});

            
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