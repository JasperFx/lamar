using System;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public partial class ServiceRegistry
    {
        public class InverseInstanceExpression<TImpl> where TImpl : class
        {
            public InverseInstanceExpression(ServiceRegistry parent)
            {
                _parent = parent;
            }

            private readonly ServiceRegistry _parent;

            public ScopedInverseInstanceExpression<TImpl> Named(string name)
            {
                return new ScopedInverseInstanceExpression<TImpl>(_parent, name);
            }

            public ScopedInverseInstanceExpression<TImpl> Scoped()
            {
                return new ScopedInverseInstanceExpression<TImpl>(_parent, ServiceLifetime.Scoped);
            }

            public ScopedInverseInstanceExpression<TImpl> Singleton()
            {
                return new ScopedInverseInstanceExpression<TImpl>(_parent, ServiceLifetime.Singleton);
            }

            public ScopedInverseInstanceExpression<TImpl> Transient()
            {
                return new ScopedInverseInstanceExpression<TImpl>(_parent, ServiceLifetime.Transient);
            }
        }

        public class ScopedInverseInstanceExpression<TImpl> where TImpl : class
        {
            public ScopedInverseInstanceExpression(ServiceRegistry parent, string name)
            {
                _parent = parent;
                _name = name;
            }

            public ScopedInverseInstanceExpression(ServiceRegistry parent, ServiceLifetime lifetime)
            {
                _parent = parent;
                _lifetime = lifetime;
            }

            private ServiceRegistry _parent;

            private string _name;
            private ServiceLifetime _lifetime;
            private ConstructorInstance<TImpl> _rootInstance;

            public ScopedInverseInstanceExpression<TImpl> For<TService>()
                where TService : class
            {
                if (!typeof(TService).IsAssignableFrom(typeof(TImpl)))
                {
                    throw new ArgumentException($"There is no conversion from {typeof(TImpl).Name} to {typeof(TImpl).Name}");
                }

                AddRootRegistration();

                var instanceExpression = _parent.For<TService>();
                Instance instance;

                if (String.IsNullOrEmpty(_name))
                {
                    instance = instanceExpression.Use(c => (TService)c.GetInstance(typeof(TImpl)));
                }
                else
                {
                    instance = instanceExpression.Use(c => (TService)c.GetInstance(typeof(TImpl), _name));
                }

                instance.Lifetime = _lifetime;

                return this;
            }

            private void AddRootRegistration()
            {
                if (_rootInstance is null)
                {
                    _rootInstance = _parent.For<TImpl>().Use<TImpl>();

                    _rootInstance.Lifetime = _lifetime;
                    if (!String.IsNullOrEmpty(_name))
                    {
                        _rootInstance.Named(_name);
                    }
                }
            }
        }
    }
}