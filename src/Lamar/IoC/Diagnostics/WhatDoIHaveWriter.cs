using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.Diagnostics;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Diagnostics
{
    public enum WhatDoIHaveDisplay
    {
        Summary,
        BuildPlan
    }
    
    public class WhatDoIHaveWriter
    {
        private readonly IModel _graph;

        public WhatDoIHaveWriter(IModel graph)
        {
            _graph = graph;
        }

        public string GetText(ModelQuery query, string title = null, WhatDoIHaveDisplay display = WhatDoIHaveDisplay.Summary)
        {
            using (var writer = new StringWriter())
            {
                if (title.IsNotEmpty())
                {
                    writer.WriteLine(title);
                }

                writer.WriteLine("");

                var model = _graph;

                var serviceTypes = query.Query(model);

                writeContentsOfServiceTypes(serviceTypes, writer, display);

                return writer.ToString();
            }
        }

        private void writeContentsOfServiceTypes(IEnumerable<IServiceFamilyConfiguration> serviceTypes,
            StringWriter writer, WhatDoIHaveDisplay display)
        {
            if (display == WhatDoIHaveDisplay.Summary)
            {
                writeSummary(serviceTypes, writer);
            }
            else
            {
                writeBuildPlan(serviceTypes, writer);
            }
        }

        private static void writeBuildPlan(IEnumerable<IServiceFamilyConfiguration> serviceTypes, StringWriter writer)
        {
            foreach (var serviceType in serviceTypes.Where(x => !x.ServiceType.IsOpenGeneric()))
            {
                writer.WriteLine("------------------------------------------------------------------------");
                writer.WriteLine($"Service Type: {serviceType.ServiceType.FullNameInCode()}");

                foreach (var instance in serviceType.Instances)
                {
                    writer.WriteLine($"Implementation Type: {instance.ImplementationType.FullNameInCode()}");
                    writer.WriteLine($"Instance Name: '{instance.Name}'");
                    writer.WriteLine();
                    writer.WriteLine(instance.DescribeBuildPlan());
                    writer.WriteLine();
                }
            }
        }

        private void writeSummary(IEnumerable<IServiceFamilyConfiguration> serviceTypes, StringWriter writer)
        {
            var reportWriter = new TextReportWriter(5);

            reportWriter.AddDivider('=');
            reportWriter.AddText("ServiceType", "Namespace", "Lifecycle", "Description", "Name");

            serviceTypes.Where(x => x.Instances.Any()).OrderBy(x => x.ServiceType.Name)
                .Each(svc => writeServiceType(svc, reportWriter));

            reportWriter.AddDivider('=');

            reportWriter.Write(writer);
        }

        private void writeServiceType(IServiceFamilyConfiguration serviceType, TextReportWriter reportWriter)
        {
            reportWriter.AddDivider('-');

            var name = serviceType.ServiceType.ShortNameInCode();
            var ns = serviceType.ServiceType.Namespace;

            var contents = new[]
            {
                name,
                ns,
                string.Empty,
                string.Empty,
                string.Empty
            };
            
            if (name.Length > 75)
            {
                contents[0] = contents[1] = string.Empty;
                reportWriter.AddContent("ServiceType: " + name);
                reportWriter.AddContent("  Namespace: " + ns);
            }
            
            var instances = serviceType.Instances.ToArray();
            var instanceRegistry = new List<InstanceRef>(instances.Length);

            setContents(contents, instances[0], instanceRegistry);
            reportWriter.AddText(contents);

            for (int i = 1; i < serviceType.Instances.Count(); i++)
            {
                writeInstance(instances[i], serviceType, reportWriter, instanceRegistry);
            }
        }

        private void writeInstance(InstanceRef instance, IServiceFamilyConfiguration serviceType,
            TextReportWriter reportWriter,
            List<InstanceRef> instanceRegistry)
        {
            if (instanceRegistry.Contains(instance) || instance == null)
            {
                return;
            }

            var contents = new[] {string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};

            setContents(contents, instance, instanceRegistry);

            reportWriter.AddText(contents);
        }


        private void setContents(string[] contents, InstanceRef instance, List<InstanceRef> instanceRegistry)
        {
            contents[2] = instance.Lifetime.ToString();

            contents[3] = instance.ToString().Elid(75);

            contents[4] = instance.Name.Elid(25);

            instanceRegistry.Add(instance);
        }
    }
}