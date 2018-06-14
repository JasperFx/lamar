using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class DefaultConventionScanner : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes).Where(type => type.GetConstructors().Any()))
            {
                var pluginType = FindPluginType(type);
                if (pluginType != null)
                {
                    services.AddType(pluginType, type);
                }
            }
        }

        public virtual Type FindPluginType(Type concreteType)
        {
            var interfaceName = "I" + concreteType.Name;
            return concreteType.GetTypeInfo().GetInterfaces().FirstOrDefault(t => t.Name == interfaceName);
        }

        public override string ToString()
        {
            return "Default I[Name]/[Name] registration convention";
        }
    }
}
