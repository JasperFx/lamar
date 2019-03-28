using System;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Util;

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

            if (_topLevelScope.Services.ContainsKey(Hash))
            {
                _service = (T) _topLevelScope.Services[Hash];
                return _service;
            }

            lock (_locker)
            {
                if (_service == null)
                {
                    if (_topLevelScope.Services.ContainsKey(Hash))
                    {
                        _service = (T) _topLevelScope.Services[Hash];
                    }
                    else
                    {
                        _service = Build(_topLevelScope);
                        if (_service is IDisposable disposable)
                        {
                            _topLevelScope.Disposables.Add(disposable);
                        }

                        _topLevelScope.Services.SmartAdd(Hash, _service);
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