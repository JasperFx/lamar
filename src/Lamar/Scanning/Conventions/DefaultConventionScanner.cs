using System;
using System.Linq;
using System.Reflection;
using BaselineTypeDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class DefaultConventionScanner : IRegistrationConvention
    {
        public OverwriteBehavior Overwrites { get; set; } = OverwriteBehavior.NewType;

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes)
                .Where(type => type.GetConstructors().Any()))
            {
                var serviceType = FindPluginType(type);
                if (serviceType != null && ShouldAdd(services, serviceType, type))
                    services.AddTransient(serviceType, type);
            }
        }

        public bool ShouldAdd(IServiceCollection services, Type serviceType, Type implementationType)
        {
            if (Overwrites == OverwriteBehavior.Always) return true;

            var matches = services.Where(x => x.ServiceType == serviceType).ToArray();
            if (!matches.Any()) return true;

            if (Overwrites == OverwriteBehavior.Never) return false;

            var hasMatch = matches.Any(x => x.Matches(serviceType, implementationType));

            return !hasMatch;
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