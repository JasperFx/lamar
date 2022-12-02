using System;
using System.Linq;
using System.Reflection;
using JasperFx.TypeDiscovery;
using LamarCodeGeneration.Util;

namespace Lamar.Scanning.Conventions
{
    internal class FindRegistriesScanner : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry registry)
        {
            types.FindTypes(TypeClassification.Closed | TypeClassification.Concretes)
                .Where(IsPublicRegistry)
                .Each(type =>
                {
                    var found = Activator.CreateInstance(type).As<ServiceRegistry>();
                    registry.IncludeRegistry(found);
                });
        }

        internal static bool IsPublicRegistry(Type type)
        {
            var ti = type.GetTypeInfo();
            if (Equals(ti.Assembly, typeof(ServiceRegistry).GetTypeInfo().Assembly)) return false;

            if (!typeof(ServiceRegistry).GetTypeInfo().IsAssignableFrom(ti)) return false;

            if (ti.IsInterface || ti.IsAbstract || ti.IsGenericType) return false;

            return type.GetConstructor(new Type[0]) != null;
        }
    }
}