using System;
using System.Collections.Generic;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Activation
{
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
}