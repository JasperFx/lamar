using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Baseline;
using Lamar.IoC;
using Lamar.IoC.Diagnostics;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;
using LamarCodeGeneration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oakton;
using Spectre.Console;

namespace Lamar.Diagnostics
{
    [Description("List all the registered Lamar services", Name = "lamar-services")]
    public class LamarServicesCommand : OaktonCommand<LamarServicesInput>
    {
        public override bool Execute(LamarServicesInput input)
        {
            AnsiConsole.Render(new FigletText("Lamar"){Color = Color.Blue});
            
            using (var host = input.BuildHost())
            {
                var container = (IContainer)host.Services;


                // TODO -- check for the interactive mode here.
                

                var configurations = input.Query(container)
                    .GroupBy(x => ResolveServiceType(x.ServiceType).Assembly)
                    .OrderBy(x => x.Key.FullName)
                    .ToArray();

                var display = input.BuildPlansFlag ? WhatDoIHaveDisplay.BuildPlan : WhatDoIHaveDisplay.Summary;

                WriteSummaries(input, configurations, display, container);
                

                

                
                
                

                // var writer = new WhatDoIHaveWriter(container.Model);
                //
                // var text = writer.GetText(query, display: display);
                //
                // if (input.FileFlag.IsNotEmpty())
                // {
                //     var fullPath = input.FileFlag.ToFullPath();
                //     Console.WriteLine("Writing the query results to " + fullPath);
                //     
                //     File.WriteAllText(fullPath,text);
                // }
                // else
                // {
                //     Console.WriteLine(text);
                // }
            }

            return true;
        }

        private void WriteSummaries(LamarServicesInput input, IGrouping<Assembly, IServiceFamilyConfiguration>[] configurations, WhatDoIHaveDisplay display,
            IContainer container)
        {
            if (display == WhatDoIHaveDisplay.Summary)
            {
                AnsiConsole.MarkupLine("[bold]Key:[/] ");

                var rule = new Rule($"[blue]Assembly Name (Assembly Version)[/]")
                    {Alignment = Justify.Left};
                AnsiConsole.Render(rule);


                var node = new Tree("{Service Type Namespace}");
                node.AddNode("{Service Type Full Name}").AddNode("{Lifetime}: {Description of Registration}");
                AnsiConsole.Render(node);
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
            if (displayMode == WhatDoIHaveDisplay.Summary)
            {
                var rule = new Rule($"[blue]{@group.Key.GetName().Name} ({@group.Key.GetName().Version})[/]")
                    {Alignment = Justify.Left};
                AnsiConsole.Render(rule);
            }

            var namespaces = @group.GroupBy(x => ResolveServiceType(x.ServiceType).Namespace);
            foreach (var ns in namespaces)
            {
                WriteNamespaceServices(input, ns, displayMode, container);
            }

            Console.WriteLine();
        }

        private void WriteNamespaceServices(LamarServicesInput input, IGrouping<string, IServiceFamilyConfiguration> ns,
            WhatDoIHaveDisplay displayMode, IContainer container)
        {
            if (displayMode == WhatDoIHaveDisplay.Summary)
            {
                var top = new Tree(ns.Key);

                foreach (var configuration in ns)
                {
                    var node = top.AddNode(configuration.ServiceType.CleanFullName());
                    WriteInstances(node, configuration, input, displayMode, container);
                }

                AnsiConsole.Render(top);
            }
            else
            {
                foreach (var configuration in ns.Where(x => !x.ServiceType.IsOpenGeneric()))
                {
                    WriteBuildPlans(configuration, container);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }

        private void WriteBuildPlans(IServiceFamilyConfiguration configuration, IContainer container)
        {
            var instanceRefs = configuration
                .Instances
                .Where(x => x.Instance is ConstructorInstance)
                .ToArray(); 
            
            if (!instanceRefs.Any())
            {
                return;
            }
            
            AnsiConsole.MarkupLine($"[blue]Service Type[/]: {configuration.ServiceType.CleanFullName()}");

              
            foreach (var instanceRef in instanceRefs)
            {
                var instance = instanceRef.Instance;

                var prefix = $"[blue]{instance.Lifetime}:[/] ";
                if (instanceRef == configuration.Default)
                {
                    prefix = "[bold]Default[/] " + prefix;
                }
                
                var description = $"{prefix}{DescriptionFor(instance)} named '{instance.Name}'";
                AnsiConsole.MarkupLine(description);
                Console.WriteLine();

                var buildPlan = instance.GetBuildPlan((Scope) container);
                Console.WriteLine(buildPlan);
                Console.WriteLine();
            }

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
                
                var description = $"[blue]{instance.Lifetime}:[/] {DescriptionFor(instance)}";
                if (input.VerboseFlag)
                {
                    description += $" named '{instance.Name}'";
                }
                
                parent.AddNode(description);
            }
            else
            {
                var table = new Table();
                table.AddColumns("Name", "Description", "Lifetime");
                foreach (var instance in configuration.Instances)
                {
                    if (configuration.Default == instance)
                    {
                        table.AddRow(instance.Name.BoldText() + " (Default)", DescriptionFor(instance.Instance).BoldText(),
                            instance.Lifetime.BoldText());
                    }
                    else
                    {
                        table.AddRow(instance.Name, DescriptionFor(instance.Instance),
                            instance.Lifetime.ToString());
                    }
                }

                parent.AddNode(table);
            }
        }

        public static string DescriptionFor(Instance instance)
        {
            if (IsOption(instance.ServiceType, out var optionType))
            {
                if (instance.ImplementationType.Closes(typeof(OptionsManager<>)))
                {
                    return $"IOptions<{optionType.FullNameInCode()}>";
                }
            }
            
            if (IsLogger(instance.ServiceType, out var loggedType))
            {
                if (instance.ImplementationType.Closes(typeof(Logger<>)))
                {
                    return $"ILogger<{loggedType.FullNameInCode()}>";
                }
            }

            if (instance is ObjectInstance o)
            {
                return "User Supplied: " + o.Service?.ToString() ?? "Null";
            }

            if (instance is ConstructorInstance c)
            {
                string text = $"new {c.ImplementationType.CleanFullName()}()";
            
                if (c.Constructor != null)
                {
                    text = $"new {c.ImplementationType.CleanFullName()}({c.Constructor.GetParameters().Select(x => x.Name).Join(", ")})";
                }

                return text;
            }

            return instance.ToString();
        }

        public static bool IsEnumerable(Type type, out Type elementType)
        {
            if (type.Closes(typeof(IEnumerable<>)) && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }
        
        public static bool IsLogger(Type type, out Type innerType)
        {
            if (type.Closes(typeof(ILogger<>)) && type.GetGenericTypeDefinition() == typeof(ILogger<>))
            {
                innerType = type.GetGenericArguments()[0];
                return true;
            }

            innerType = null;
            return false;
        }

        public static bool IsOption(Type type, out Type optionType)
        {
            if (type.Closes(typeof(IOptions<>)) && type != typeof(IOptions<>))
            {
                optionType = type.GetGenericArguments().First();
                return true;
            }

            optionType = null;
            return false;

        }

        public static Assembly AssemblyForType(Type type)
        {
            if (IsEnumerable(type, out var elementType))
            {
                return AssemblyForType(elementType);
            }

            if (IsOption(type, out var optionType))
            {
                return AssemblyForType(optionType);
            }

            if (IsLogger(type, out var loggedType))
            {
                return AssemblyForType(loggedType);
            }

            return type.Assembly;
        }

        public static Type ResolveServiceType(Type type)
        {
            if (IsEnumerable(type, out var elementType))
            {
                return ResolveServiceType(elementType);
            }

            if (IsOption(type, out var optionType))
            {
                return ResolveServiceType(optionType);
            }

            if (IsLogger(type, out var loggedType))
            {
                return ResolveServiceType(loggedType);
            }

            return type;
        }

        private static readonly IList<Type> _ignoredBaseTypes = new List<Type>
        {
            typeof(IValidateOptions<>),
            typeof(IConfigureOptions<>),
            typeof(IPostConfigureOptions<>),
            typeof(IOptionsFactory<>),
            typeof(IOptionsMonitor<>),
            typeof(IOptionsMonitorCache<>),
            typeof(IOptionsChangeTokenSource<>),
        };

        public static bool IgnoreIfNotVerbose(Type type)
        {
            if (_ignoredBaseTypes.Any(type.Closes)) return true;

            if (IsEnumerable(type, out var elementType))
            {
                return IgnoreIfNotVerbose(elementType);
            }
            
            return false;
        }
    }
    
    public static class AnsiConsoleExtensions
    {
        public static string CleanFullName(this Type type)
        {
            try
            {
                if (type.IsOpenGeneric())
                {
                    var parts = type.FullNameInCode().Split('`');
                    var argCount = int.Parse(parts[1]) - 1;

                    return $"{parts[0]}<{"".PadLeft(argCount, ',')}>";
                }
                else if (LamarServicesCommand.IsEnumerable(type, out var elementType))
                {
                    return $"IEnumerable<{elementType.FullNameInCode()}>";
                }
                else if (LamarServicesCommand.IsOption(type, out var optionType))
                {
                    return $"IOptions<{optionType.FullNameInCode()}>";
                }
                else if (LamarServicesCommand.IsLogger(type, out var loggedType))
                {
                    return $"ILogger<{optionType.FullNameInCode()}>";
                }
                else
                {
                    return type.FullNameInCode();
                }
            }
            catch (Exception)
            {
                return type?.FullName;
            }
        }
        
        public static string BoldText(this object data)
        {
            return $"[bold]{data}[/]";
        }


    }
}