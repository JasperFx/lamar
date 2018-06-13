using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.IoC;

namespace Lamar
{
    internal class ServiceFamilyConfiguration : IServiceFamilyConfiguration
    {
        private readonly ServiceFamily _family;
        private readonly Scope _scope;

        public ServiceFamilyConfiguration(ServiceFamily family, Scope scope)
        {
            _family = family;
            _scope = scope;
        }

        public Type ServiceType => _family.ServiceType;
        public InstanceRef Default => _family.Default == null ? null : new InstanceRef(_family.Default, _scope);
        public IEnumerable<InstanceRef> Instances => _family.All.Select(x => new InstanceRef(x, _scope));
        public bool HasImplementations() => _family.All.Any();
    }
}