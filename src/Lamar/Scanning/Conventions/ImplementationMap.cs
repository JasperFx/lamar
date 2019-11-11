using System;
using System.Linq;
using BaselineTypeDiscovery;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class ImplementationMap : IRegistrationConvention
    {
        private readonly ServiceLifetime _lifetime;

        public ImplementationMap(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _lifetime = lifetime;
        }

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            var interfaces = types.FindTypes(TypeClassification.Interfaces | TypeClassification.Closed)
                .Where(x => x != typeof(IDisposable));
            var concretes = types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed)
                .Where(x => x.GetConstructors().Any()).ToArray();

            interfaces.Each(@interface =>
            {
                var implementors = concretes.Where(x => x.CanBeCastTo(@interface)).ToArray();
                if (implementors.Count() == 1) services.Add(new ServiceDescriptor(@interface, implementors.Single(), _lifetime));
            });
        }

        public override string ToString()
        {
            return "Register any single implementation of any interface against that interface";
        }
    }
}