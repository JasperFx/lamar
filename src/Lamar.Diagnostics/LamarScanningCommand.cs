using System;
using LamarCodeGeneration.Util;
using Oakton;

namespace Lamar.Diagnostics
{
    [Description("Runs Lamar's type scanning diagnostics", Name = "lamar-scanning")]
    public class LamarScanningCommand : OaktonCommand<NetCoreInput>
    {
        public override bool Execute(NetCoreInput input)
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