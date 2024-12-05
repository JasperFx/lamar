using System;

namespace Lamar.IoC.Resolvers;

public abstract class TransientResolver<T> : IResolver
{
    public object Resolve(Scope scope)
    {
        var service = Build(scope);
        scope.TryAddDisposable(service);

        return service;
    }

    public Type ServiceType => typeof(T);

    public string Name { get; set; }
    public InstanceIdentifier Hash { get; set; }

    public abstract T Build(Scope scope);
}