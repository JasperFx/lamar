using System;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar;

public partial class ServiceRegistry
{
    public class ProvidedInstanceInverseInstanceExpression<TImpl> : BaseInverseInstanceExpression<TImpl>
        where TImpl : class
    {
        private readonly TImpl _instance;

        public ProvidedInstanceInverseInstanceExpression(ServiceRegistry parent, TImpl instance)
            : base(parent)
        {
            _instance = instance;
        }

        public override ScopedInverseInstanceExpression<TImpl> Named(string name)
        {
            return new ScopedInverseInstanceExpression<TImpl>(Parent, _instance, name);
        }

        public override ScopedInverseInstanceExpression<TImpl> Singleton()
        {
            return new ScopedInverseInstanceExpression<TImpl>(Parent, _instance);
        }
    }

    public class InverseInstanceExpression<TImpl> : BaseInverseInstanceExpression<TImpl>
        where TImpl : class
    {
        public InverseInstanceExpression(ServiceRegistry parent)
            : base(parent)
        {
        }

        public ScopedInverseInstanceExpression<TImpl> Transient()
        {
            return new ScopedInverseInstanceExpression<TImpl>(Parent, ServiceLifetime.Transient);
        }

        public ScopedInverseInstanceExpression<TImpl> Scoped()
        {
            return new ScopedInverseInstanceExpression<TImpl>(Parent, ServiceLifetime.Scoped);
        }
    }

    public class BaseInverseInstanceExpression<TImpl> where TImpl : class
    {
        public BaseInverseInstanceExpression(ServiceRegistry parent)
        {
            Parent = parent;
        }

        protected ServiceRegistry Parent { get; }

        public virtual ScopedInverseInstanceExpression<TImpl> Named(string name)
        {
            return new ScopedInverseInstanceExpression<TImpl>(Parent, name);
        }

        public virtual ScopedInverseInstanceExpression<TImpl> Singleton()
        {
            return new ScopedInverseInstanceExpression<TImpl>(Parent, ServiceLifetime.Singleton);
        }
    }

    public class ScopedInverseInstanceExpression<TImpl> where TImpl : class
    {
        private readonly TImpl _providedInstance;
        private readonly ServiceLifetime _lifetime;
        private readonly string _name;
        private readonly ServiceRegistry _parent;
        private Instance _rootInstance;

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

        public ScopedInverseInstanceExpression(ServiceRegistry parent, TImpl instance, string name)
        {
            _parent = parent;
            _providedInstance = instance;
            _name = name;
        }

        public ScopedInverseInstanceExpression(ServiceRegistry parent, TImpl instance)
        {
            _parent = parent;
            _providedInstance = instance;

            _lifetime = ServiceLifetime.Singleton;
        }

        public ScopedInverseInstanceExpression<TImpl> For<TService>()
            where TService : class
        {
            if (!typeof(TService).IsAssignableFrom(typeof(TImpl)))
            {
                throw new ArgumentException(
                    $"There is no conversion from {typeof(TImpl).Name} to {typeof(TImpl).Name}");
            }

            AddRootRegistration();

            var instanceExpression = _parent.For<TService>();
            Instance instance;

            if (string.IsNullOrEmpty(_name))
            {
                instance = instanceExpression.Use(c => (TService)c.GetInstance(typeof(TImpl)));
            }
            else
            {
                instance = instanceExpression.Use(c => (TService)c.GetInstance(typeof(TImpl), _name)).Named(_name);
            }

            instance.Lifetime = _lifetime;

            return this;
        }

        private void AddRootRegistration()
        {
            if (_rootInstance is null)
            {
                if (_providedInstance is null)
                {
                    _rootInstance = _parent.For<TImpl>().Use<TImpl>();
                }
                else
                {
                    _rootInstance = _parent.For<TImpl>().Use(_providedInstance);
                }

                _rootInstance.Lifetime = _lifetime;
                if (!string.IsNullOrEmpty(_name))
                {
                    _rootInstance.Named(_name);
                }
            }
        }
    }
}