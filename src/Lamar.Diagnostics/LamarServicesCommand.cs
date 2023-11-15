using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JasperFx.Core;
using Lamar.IoC.Diagnostics;
using Oakton;
using Spectre.Console;

namespace Lamar.Diagnostics
{
    [Description("List all the registered Lamar services", Name = "lamar-services")]
    public class LamarServicesCommand : OaktonCommand<LamarServicesInput>
    {
        public override bool Execute(LamarServicesInput input)
        {
            if (input.FileFlag.IsNotEmpty())
            {
                AnsiConsole.Record();
            }
            
            AnsiConsole.Write(new FigletText("Lamar"){Color = Color.Blue});

            using var host = input.BuildHost();
            var container = (IContainer)host.Services;


            // TODO -- check for the interactive mode here.
                

            var configurations = input.Query(container)
                .GroupBy(x => x.ServiceType.ResolveServiceType().Assembly)
                .OrderBy(x => x.Key.FullName)
                .ToArray();

            var display = input.BuildPlansFlag ? WhatDoIHaveDisplay.BuildPlan : WhatDoIHaveDisplay.Summary;

            WriteSummaries(input, configurations, display, container);
                

                
            if (input.FileFlag.IsNotEmpty())
            {
                var fullPath = input.FileFlag.ToFullPath();
                Console.WriteLine("Writing the query results to " + fullPath);

                var extension = Path.GetExtension(fullPath);
                if (extension.EndsWith("htm", StringComparison.OrdinalIgnoreCase) ||
                    extension.EndsWith("html", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllText(fullPath,AnsiConsole.ExportHtml());
                }
                else
                {
                    File.WriteAllText(fullPath,AnsiConsole.ExportText());
                }
                    
                    
            }

            return true;
        }

        private void WriteSummaries(LamarServicesInput input, IGrouping<Assembly, IServiceFamilyConfiguration>[] configurations, WhatDoIHaveDisplay display,
            IContainer container)
        {
            if (display == WhatDoIHaveDisplay.Summary)
            {
                AnsiConsole.MarkupLine("[bold]Key:[/] ");

                var rule = new Rule($"[blue]Assembly Name (Assembly Version)[/]") {Justification = Justify.Left};
                AnsiConsole.Write(rule);


                var node = new Tree("{Service Type Namespace}");
                node.AddNode("{Service Type Full Name}").AddNode("{Lifetime}: {Description of Registration}");
                AnsiConsole.Write(node);
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Build Plans for registrations built by calling constructors");
                Console.WriteLine();
            }


            foreach (var group in configurations)
            {
                WriteAssemblyServices(input, @group, display, container);
            }
        }

        private void WriteAssemblyServices(LamarServicesInput input,
            IGrouping<Assembly, IServiceFamilyConfiguration> @group, WhatDoIHaveDisplay displayMode,
            IContainer container)
        {
            var rule = new Rule($"[blue]{@group.Key.GetName().Name} ({@group.Key.GetName().Version})[/]"){ Justification = Justify.Left};
            AnsiConsole.Write(rule);

            var namespaces = @group.GroupBy(x => x.ServiceType.ResolveServiceType().Namespace);
            foreach (var ns in namespaces)
            {
                WriteNamespaceServices(input, ns, displayMode, container);
            }

            Console.WriteLine();
        }

        private void WriteNamespaceServices(LamarServicesInput input, IGrouping<string, IServiceFamilyConfiguration> ns,
            WhatDoIHaveDisplay displayMode, IContainer container)
        {
            var top = new Tree(ns.Key);

            foreach (var configuration in ns)
            {
                var node = top.AddNode(configuration.ServiceType.CleanFullName().EscapeMarkup());
                WriteInstances(node, configuration, input, displayMode, container);
            }

            AnsiConsole.Write(top);

            Console.WriteLine();
        }

        private void WriteInstances(TreeNode parent, IServiceFamilyConfiguration configuration,
            LamarServicesInput input, WhatDoIHaveDisplay displayMode, IContainer container)
        {
            if (!configuration.Instances.Any())
            {   
                parent.AddNode("None");
                return;
            }

            if (configuration.Instances.Count() == 1)
            {
                var instance = configuration.Default.Instance;

                if (displayMode == WhatDoIHaveDisplay.Summary)
                {
                    parent.WriteSingleInstanceNode(input, instance, true);
                }
                else
                {
                    parent.WriteBuildPlanNode(input, instance, true);
                }
            }
            else if (displayMode == WhatDoIHaveDisplay.Summary)
            {
                parent.WriteInstancesTable(configuration, displayMode);
            }
            else
            {
                foreach (var instanceRef in configuration.Instances)
                {
                    var isDefault = configuration.Default.Instance == instanceRef.Instance;
                    
                    parent.WriteBuildPlanNode(input, instanceRef.Instance, isDefault);
                }
            }
        }


    }
}