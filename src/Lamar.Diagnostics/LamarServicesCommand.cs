using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Baseline;
using Lamar.IoC.Diagnostics;
using Oakton;
using TypeExtensions = LamarCodeGeneration.Util.TypeExtensions;

namespace Lamar.Diagnostics
{
    [Description("List all the registered Lamar services", Name = "lamar-services")]
    public class LamarServicesCommand : OaktonCommand<LamarServicesInput>
    {
        public override bool Execute(LamarServicesInput input)
        {
            using (var host = input.BuildHost())
            {
                var container = TypeExtensions.As<IContainer>(host.Services);



                var query = input.ToModelQuery(container);

                var display = input.BuildPlansFlag ? WhatDoIHaveDisplay.BuildPlan : WhatDoIHaveDisplay.Summary;

                var writer = new WhatDoIHaveWriter(container.Model);

                var text = writer.GetText(query, display: display);
                
                if (input.FileFlag.IsNotEmpty())
                {
                    var fullPath = input.FileFlag.ToFullPath();
                    Console.WriteLine("Writing the query results to " + fullPath);
                    
                    File.WriteAllText(fullPath,text);
                }
                else
                {
                    Console.WriteLine(text);
                }
            }

            return true;
        }


    }
}