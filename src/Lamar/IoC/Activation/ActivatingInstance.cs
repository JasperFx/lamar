using System;
using System.Collections.Generic;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Activation
{
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
}