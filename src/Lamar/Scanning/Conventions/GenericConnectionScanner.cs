using System;
using System.Collections.Generic;
using System.Linq;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using LamarCodeGeneration.Util;

namespace Lamar.Scanning.Conventions
{
    public class GenericConnectionScanner : IRegistrationConvention
    {
        private readonly IList<Type> _concretions = new List<Type>();
        private readonly IList<Type> _interfaces = new List<Type>();
        private readonly Type _openType;

        public GenericConnectionScanner(Type openType)
        {
            _openType = openType;

            if (!_openType.IsOpenGeneric())
            {
                throw new InvalidOperationException("This scanning convention can only be used with open generic types");
            }
        }

        public override string ToString()
        {
            return "Connect all implementations of open generic type " + _openType.FullNameInCode();
        }

        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.AllTypes())
            {
                var interfaceTypes = type.FindInterfacesThatClose(_openType).ToArray();
                if (!interfaceTypes.Any()) continue;

                if (type.IsConcrete())
                {
                    _concretions.Add(type);
                }

                foreach (var interfaceType in interfaceTypes)
                {
                    _interfaces.Fill(interfaceType);
                }
            }


            foreach (var @interface in _interfaces)
            {
                var exactMatches = _concretions.Where(x => x.CanBeCastTo(@interface)).ToArray();
                foreach (var type in exactMatches)
                {
                    services.AddTransient(@interface, type);
                }

                if (!@interface.IsOpenGeneric())
                {
                    addConcretionsThatCouldBeClosed(@interface, services);
                }
            }

            var concretions = services.ConnectedConcretions();
            foreach (var type in _concretions)
            {
                concretions.Fill(type);
            }

        }

        private void addConcretionsThatCouldBeClosed(Type @interface, IServiceCollection services)
        {
            _concretions.Where(x => x.IsOpenGeneric())
                .Where(x => x.CouldCloseTo(@interface))
                .Each(type =>
                {
                    try
                    {

                        services.AddTransient(@interface, type.MakeGenericType(@interface.GetGenericArguments()));
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