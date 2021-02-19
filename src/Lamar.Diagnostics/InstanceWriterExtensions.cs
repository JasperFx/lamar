using System.Linq;
using Baseline;
using Lamar.IoC.Diagnostics;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace Lamar.Diagnostics
{
    public static class InstanceWriterExtensions
    {
        public static void WriteSingleInstanceNode(this TreeNode parent, LamarServicesInput input, Instance instance,
            WhatDoIHaveDisplay displayMode,
            bool isDefault)
        {

            var description = $"[blue]{instance.Lifetime}:[/] {instance.ToDescription()}";
            if (input.VerboseFlag)
            {
                description += $" named '{instance.Name}'";
            }

            if (isDefault)
            {
                description = description.BoldText();
            }

            parent.AddNode(description);
        }
        
        public static void WriteMultipleInstanceNodes(this TreeNode parent, IServiceFamilyConfiguration configuration,
            WhatDoIHaveDisplay displayMode)
        {
            var table = new Table();
            table.AddColumns("Name", "Description", "Lifetime");
            foreach (var instance in configuration.Instances)
            {
                if (configuration.Default.Instance.Equals(instance.Instance))
                {
                    table.AddRow((instance.Name+ " (Default)").BoldText() , instance.Instance.ToDescription().BoldText(),
                        instance.Lifetime.BoldText());
                }
                else
                {
                    table.AddRow(instance.Name, instance.Instance.ToDescription(),
                        instance.Lifetime.ToString());
                }
            }

            parent.AddNode(table);
        }
        
        public static string ToDescription(this Instance instance)
        {
            if (instance.ServiceType.IsOption(out var optionType))
            {
                if (instance.ImplementationType.Closes(typeof(OptionsManager<>)))
                {
                    return $"IOptions<{optionType.FullNameInCode()}>";
                }
            }
            
            if (instance.ServiceType.IsLogger(out var loggedType))
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

    }
}