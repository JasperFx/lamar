using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LamarCodeGeneration;
using LamarCompiler;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Diagnostics
{
    public class ModelQuery
    {
        public string Namespace;
        public Type ServiceType;
        public Assembly Assembly;
        public string TypeName;

        public IEnumerable<IServiceFamilyConfiguration> Query(IModel model)
        {
            var enumerable = model.ServiceTypes;

            if (Namespace.IsNotEmpty())
            {
                enumerable = enumerable.Where(x => x.ServiceType.IsInNamespace(Namespace));
            }

            if (ServiceType != null)
            {
                enumerable = enumerable.Where(x => x.ServiceType == ServiceType);
            }

            if (Assembly != null)
            {
                enumerable = enumerable.Where(x => x.ServiceType.GetTypeInfo().Assembly == Assembly);
            }

            if (TypeName.IsNotEmpty())
            {
                enumerable = enumerable.Where(x => x.ServiceType.Name.ToLower().Contains(TypeName.ToLower()));
            }

            return enumerable;
        }
    }

    public class WhatDoIHaveWriter
    {
        private readonly IModel _graph;

        public WhatDoIHaveWriter(IModel graph)
        {
            _graph = graph;
        }

        public string GetText(ModelQuery query, string title = null)
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

                writeContentsOfServiceTypes(serviceTypes, writer);

                return writer.ToString();
            }
        }

        private void writeContentsOfServiceTypes(IEnumerable<IServiceFamilyConfiguration> serviceTypes,
            StringWriter writer)
        {
            var reportWriter = new TextReportWriter(5);

            reportWriter.AddDivider('=');
            reportWriter.AddText("ServiceType", "Namespace", "Lifecycle", "Description", "Name");

            serviceTypes.Where(x => x.Instances.Any()).OrderBy(x => x.ServiceType.Name).Each(svc => writeServiceType(svc, reportWriter));

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