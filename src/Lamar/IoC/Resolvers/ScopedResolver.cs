using System;
using JasperFx.Core;

namespace Lamar.IoC.Resolvers;

public abstract class ScopedResolver<T> : IResolver
{
    private readonly object _locker = new();
    public Type ServiceType => typeof(T);

    public object Resolve(Scope scope)
    {
        if (scope.Services.TryFind(Hash, out var service))
        {
            return service;
        }

        lock (_locker)
        {
            if (scope.Services.TryFind(Hash, out service))
            {
                return service;
            }

            service = Build(scope);
            scope.Services = scope.Services.AddOrUpdate(Hash, service);

            scope.TryAddDisposable(service);

            return service;
        }
    }

    public string Name { get; set; }
    public InstanceIdentifier Hash { get; set; }

    public abstract T Build(Scope scope);
}