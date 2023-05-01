using System;
using System.Linq;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Lazy;

internal class FuncOrLazyPolicy : IFamilyPolicy
{
    public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
    {
        if (type.Closes(typeof(Func<>)))
        {
            return new ServiceFamily(type, new IDecoratorPolicy[0],
                typeof(FuncInstance<>).CloseAndBuildAs<Instance>(type.GetGenericArguments().Single()));
        }

        if (type.Closes(typeof(Lazy<>)))
        {
            return new ServiceFamily(type, new IDecoratorPolicy[0],
                typeof(LazyInstance<>).CloseAndBuildAs<Instance>(type.GetGenericArguments().Single()));
        }

        if (type.Closes(typeof(Func<,>)) && type.GenericTypeArguments.First() == typeof(string))
        {
            return new ServiceFamily(type, new IDecoratorPolicy[0],
                typeof(FuncByNameInstance<>).CloseAndBuildAs<Instance>(type.GetGenericArguments().Last()));
        }

        return null;
    }
}