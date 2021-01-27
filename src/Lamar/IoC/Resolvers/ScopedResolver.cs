﻿using System;

namespace Lamar.IoC.Resolvers
{
    public abstract class ScopedResolver<T> : IResolver
    {
        public Type ServiceType => typeof(T);
        
        private readonly object _locker = new object();

        public object Resolve(Scope scope)
        {
            if (scope.Services.TryGetValue(Hash, out object service))
            {
                return service;
            }

            lock (_locker)
            {
                if (scope.Services.TryGetValue(Hash, out service))
                {
                    return service;
                }
                
                service = (T) Build(scope);
                scope.Services.TryAdd(Hash, service);

                if (service is IDisposable)
                {
                    scope.Disposables.Add((IDisposable) service);
                }

                return service;
            }
            

        }

        public abstract T Build(Scope scope);

        public string Name { get; set; }
        public int Hash { get; set; }
    }
}