using System;
using System.Collections.Generic;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Activation
{
    internal class ActivationPolicy<T> : IDecoratorPolicy
    {
        private readonly Action<IServiceContext,T> _action;

        public ActivationPolicy(Action<IServiceContext, T> action)
        {
            _action = action;
        }

        public virtual bool TestInstance(Instance inner)
        {
            if (inner.ServiceType == typeof(T))
            {
                return true;
            }

            if (inner.ImplementationType == null) return false;

            return inner.ImplementationType.CanBeCastTo<T>();
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

    }
    
    internal class ActivatingInstance<TActual, TService> : LambdaInstance<Scope, TService>
    {
        private readonly Instance _inner;

        public ActivatingInstance(Action<IServiceContext, TActual> action, Instance inner)
            : base(inner.ServiceType, buildCreator(action, inner), inner.Lifetime)
        {
            inner.Lifetime = ServiceLifetime.Transient;
            _inner = inner;
        }
        
        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _inner.CreatePlan(services);
            foreach (var message in _inner.ErrorMessages)
            {
                ErrorMessages.Add(message);
            }
            
            return base.createPlan(services);
        }
        
        

        internal override string GetBuildPlan(Scope rootScope)
        {
            return $"User defined interception{Environment.NewLine}{base.GetBuildPlan(rootScope)}";
        }

        private static Func<Scope, TService> buildCreator(Action<IServiceContext, TActual> interceptor, Instance inner)
        {
            switch (inner.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return s =>
                    {
                        var raw = inner.QuickResolve(s);
                        if (raw is TActual a) interceptor(s, a);
                        return (TService) raw;
                    };
                
                default:

                    return s =>
                    {
                        var raw = inner.Resolve(s);
                        if (raw is TActual a) interceptor(s, a);
                        return (TService) raw;
                    };
            }
        }
    }
    

    internal class InterceptingInstance<T> : LambdaInstance<Scope, T>
    {
        private readonly Instance _inner;

        public InterceptingInstance(Func<IServiceContext, T, T> interceptor, Instance inner)
            : base(inner.ServiceType, buildCreator(interceptor, inner), inner.Lifetime)
        {
            inner.Lifetime = ServiceLifetime.Transient;
            _inner = inner;
        }

        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _inner.CreatePlan(services);
            foreach (var message in _inner.ErrorMessages)
            {
                ErrorMessages.Add(message);
            }
            
            return base.createPlan(services);
        }
        
        

        internal override string GetBuildPlan(Scope rootScope)
        {
            return $"User defined interception{Environment.NewLine}{base.GetBuildPlan(rootScope)}";
        }

        private static Func<Scope, T> buildCreator(Func<IServiceContext, T, T> interceptor, Instance inner)
        {
            switch (inner.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return s =>
                    {
                        var raw = inner.QuickResolve(s);
                        return raw switch
                        {
                            T inner => interceptor(s.Root, inner),
                            null => throw new InvalidOperationException(
                                $"Inner instance {inner} of activator returned null"),
                            _ => throw new InvalidCastException(
                                $"Activation interceptor expected type {typeof(T).FullNameInCode()}, but was {raw.GetType().FullNameInCode()}")
                        };
                    };
                
                default:

                    return s =>
                    {
                        var raw = inner.Resolve(s);
                        return raw switch
                        {
                            T inner => interceptor(s.Root, inner),
                            null => throw new InvalidOperationException(
                                $"Inner instance {inner} of activator returned null"),
                            _ => throw new InvalidCastException(
                                $"Activation interceptor expected type {typeof(T).FullNameInCode()}, but was {raw.GetType().FullNameInCode()}")
                        };
                    };
            }
        }
    }
    
    internal class InterceptorPolicy<T> : IDecoratorPolicy
    {
        private readonly Func<IServiceContext, T, T> _interceptor;

        public InterceptorPolicy(Func<IServiceContext, T, T> interceptor)
        {
            _interceptor = interceptor;
        }

        public virtual bool TestInstance(Instance inner)
        {
            return inner.ServiceType == typeof(T);
        }

        public bool TryWrap(Instance inner, out Instance wrapped)
        {
            if (TestInstance(inner))
            {
                wrapped = new InterceptingInstance<T>(_interceptor, inner);

                return true;
            }

            wrapped = null;
            return false;
        }

    }

}