using System;
using Lamar.Util;

namespace Lamar.IoC.Resolvers
{
    public abstract class SingletonResolver<T> : IResolver 
    {
        private readonly Scope _topLevelScope;
        private readonly object _locker = new object();
        
        public Type ServiceType => typeof(T);

        private T _service;
        
        public SingletonResolver(Scope topLevelScope)
        {
            _topLevelScope = topLevelScope;
        }

        public object Resolve(Scope scope)
        {
            if (_service != null) return _service;


            if (_topLevelScope.Services.TryFind(Hash, out var service))
            {
                _service = (T) service;
                return _service;
            }

            lock (_locker)
            {
                if (_service == null)
                {
                    if (_topLevelScope.Services.TryFind(Hash, out var o))
                    {
                        _service = (T) o;
                    }
                    else
                    {
                        _service = Build(_topLevelScope);
                        _topLevelScope.TryAddDisposable(_service);

                        _topLevelScope.Services = _topLevelScope.Services.AddOrUpdate(Hash, _service);
                    }
                }
            }

            return _service;
        }
        
        public abstract T Build(Scope scope);
        
        public string Name { get; set; }
        public int Hash { get; set; }

    }


}