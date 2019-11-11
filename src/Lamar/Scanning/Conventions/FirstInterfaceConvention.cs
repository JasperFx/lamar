using System;
using System.Linq;
using BaselineTypeDiscovery;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class FirstInterfaceConvention : IRegistrationConvention
    {
        private readonly ServiceLifetime _lifetime;

        public FirstInterfaceConvention(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _lifetime = lifetime;
        }

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.FindTypes(TypeClassification.Concretes).Where(x => x.GetConstructors().Any()))
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault(x => x != typeof(IDisposable));
                if (interfaceType != null && !interfaceType.HasAttribute<LamarIgnoreAttribute>() &&
                    !type.IsOpenGeneric()) services.AddType(interfaceType, type, _lifetime);
            }
        }

        public override string ToString()
        {
            return "Register all concrete types against the first interface (if any) that they implement";
        }
    }
}