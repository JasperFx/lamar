using System;
using LamarCodeGeneration.Util;
using Oakton;
using Oakton.AspNetCore;

namespace Lamar.Diagnostics
{
    [Description("Runs Lamar's type scanning diagnostics", Name = "lamar-scanning")]
    public class LamarScanningCommand : OaktonCommand<AspNetCoreInput>
    {
        public override bool Execute(AspNetCoreInput input)
        {
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