using System;
using System.Reflection;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using LamarCodeGeneration.Util;

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

            if (serviceGraph.CouldBuild(type, out var message))
            {
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, new ConstructorInstance(type, type, ServiceLifetime.Transient));
            }
            else
            {
                var empty = new ServiceFamily(type, new IDecoratorPolicy[0], new Instance[0]);
                empty.CannotBeResolvedMessage = message;

                return empty;
            }

            return null;
        }
    }
}