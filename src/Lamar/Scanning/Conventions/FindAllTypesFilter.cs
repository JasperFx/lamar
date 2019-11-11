using System;
using System.Linq;
using BaselineTypeDiscovery;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class FindAllTypesFilter : IRegistrationConvention
    {
        private readonly Type _serviceType;
        private Func<Type, string> _namePolicy = type => type.NameInCode();
        private readonly ServiceLifetime _lifetime;

        public FindAllTypesFilter(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _serviceType = serviceType;
            _lifetime = lifetime;
        }

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            if (_serviceType.IsOpenGeneric())
            {
                var scanner = new GenericConnectionScanner(_serviceType, _lifetime);
                scanner.ScanTypes(types, services);
            }
            else
            {
                types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).Where(Matches).Each(type =>
                {
                    var serviceType = determineLeastSpecificButValidType(_serviceType, type);
                    var instance = services.AddType(serviceType, type, _lifetime);
                    if (instance != null) instance.Name = _namePolicy(type);
                });
            }
        }

        public bool Matches(Type type)
        {
            return Instance.CanBeCastTo(type, _serviceType) && type.GetConstructors().Any() && type.CanBeCreated();
        }

        private static Type determineLeastSpecificButValidType(Type pluginType, Type type)
        {
            if (pluginType.IsGenericTypeDefinition && !type.IsOpenGeneric())
                return type.FindFirstInterfaceThatCloses(pluginType);

            return pluginType;
        }

        public override string ToString()
        {
            return "Find and register all types implementing " + _serviceType.FullName;
        }

        public FindAllTypesFilter NameBy(Func<Type, string> namePolicy)
        {
            _namePolicy = namePolicy;
            return this;
        }
    }
}