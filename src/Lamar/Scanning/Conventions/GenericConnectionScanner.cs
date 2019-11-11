using System;
using System.Collections.Generic;
using System.Linq;
using BaselineTypeDiscovery;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public class GenericConnectionScanner : IRegistrationConvention
    {
        private readonly IList<Type> _concretions = new List<Type>();
        private readonly IList<Type> _interfaces = new List<Type>();
        private readonly Type _openType;
        private readonly ServiceLifetime _lifetime;

        public GenericConnectionScanner(Type openType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _openType = openType;
            _lifetime = lifetime;

            if (!_openType.IsOpenGeneric())
                throw new InvalidOperationException(
                    "This scanning convention can only be used with open generic types");
        }

        public void ScanTypes(
            TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.AllTypes())
            {
                var interfaceTypes = type.FindInterfacesThatClose(_openType).ToArray();
                if (!interfaceTypes.Any()) continue;

                if (type.IsConcrete()) _concretions.Add(type);

                foreach (var interfaceType in interfaceTypes) _interfaces.Fill(interfaceType);
            }


            foreach (var @interface in _interfaces)
            {
                var exactMatches = _concretions.Where(x => x.CanBeCastTo(@interface)).ToArray();
                foreach (var type in exactMatches) services.Add(new ServiceDescriptor(@interface, type, _lifetime));

                if (!@interface.IsOpenGeneric()) addConcretionsThatCouldBeClosed(@interface, services);
            }

            var concretions = services.ConnectedConcretions();
            foreach (var type in _concretions) concretions.Fill(type);
        }

        public override string ToString()
        {
            return "Connect all implementations of open generic type " + _openType.FullNameInCode();
        }

        private void addConcretionsThatCouldBeClosed(Type @interface, IServiceCollection services)
        {
            _concretions.Where(x => x.IsOpenGeneric())
                .Where(x => x.CouldCloseTo(@interface))
                .Each(type =>
                {
                    try
                    {
                        services.Add(new ServiceDescriptor(@interface, type.MakeGenericType(@interface.GetGenericArguments()), _lifetime));
                    }
                    catch (Exception)
                    {
                        // Because I'm too lazy to fight with the bleeping type constraints to "know"
                        // if it's possible to make the generic type and this is just easier.
                    }
                });
        }
    }
}