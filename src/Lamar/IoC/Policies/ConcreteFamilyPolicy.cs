using System;
using System.Reflection;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar;

internal class ConcreteFamilyPolicy : IFamilyPolicy
{
    public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
    {
        if (type.IsGenericTypeDefinition)
        {
            return null;
        }

        if (!type.IsConcrete())
        {
            return null;
        }


        if (!IsReallyPublic(type))
        {
            return null;
        }

        if (serviceGraph.CouldBuild(type, out var message))
        {
            return new ServiceFamily(type, serviceGraph.DecoratorPolicies,
                new ConstructorInstance(type, type, ServiceLifetime.Transient));
        }

        var empty = new ServiceFamily(type, new IDecoratorPolicy[0]);
        empty.CannotBeResolvedMessage = message;

        return empty;
    }

    public static bool IsReallyPublic(Type type)
    {
        if (type.IsPublic)
        {
            return true;
        }

        if (type.MemberType == MemberTypes.NestedType)
        {
            return IsReallyPublic(type.ReflectedType);
        }

        return false;
    }
}