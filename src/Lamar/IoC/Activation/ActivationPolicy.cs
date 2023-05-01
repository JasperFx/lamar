using System;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Activation;

internal class ActivationPolicy<T> : IDecoratorPolicy
{
    private readonly Action<IServiceContext, T> _action;

    public ActivationPolicy(Action<IServiceContext, T> action)
    {
        _action = action;
    }

    public bool TryWrap(Instance inner, out Instance wrapped)
    {
        if (TestInstance(inner))
        {
            wrapped = typeof(ActivatingInstance<,>).CloseAndBuildAs<Instance>(_action, inner, typeof(T),
                inner.ServiceType);

            return true;
        }

        wrapped = null;
        return false;
    }

    public virtual bool TestInstance(Instance inner)
    {
        if (inner.ServiceType == typeof(T))
        {
            return true;
        }

        if (inner.ImplementationType == null)
        {
            return false;
        }

        return inner.ImplementationType.CanBeCastTo<T>();
    }
}