using System;
using System.Reflection;
using Lamar.IoC.Instances;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public class ConcreteFamilyPolicy : IFamilyPolicy
    {
        public static bool IsReallyPublic(Type type)
        {
            if (type.IsPublic) return true;

            if (type.MemberType == MemberTypes.NestedType)
            {
                return type.ReflectedType.IsPublic;
            }

            return false;
        }
        
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.IsGenericTypeDefinition) return null;
            if (!type.IsConcrete()) return null;
            
            
            if (!IsReallyPublic(type)) return null;

            if (serviceGraph.CouldBuild(type))
            {
                return new ServiceFamily(type, new ConstructorInstance(type, type, ServiceLifetime.Transient));
            }

            return null;
        }
    }
}