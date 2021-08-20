using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lamar.IoC.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.IoC.Instances
{
    [Obsolete("We'll replace this with a very similar option")]
    internal class InterceptingInstance<T> : Instance
    {
        private readonly Instance _inner;
        private readonly IActivationInterceptor<T> _interceptor;

        public InterceptingInstance(Instance inner, IActivationInterceptor<T> interceptor)
            : base(inner.ServiceType, inner.ImplementationType, inner.Lifetime)
        {
            _inner = inner;
            _interceptor = interceptor;
            Name = _inner.Name;
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

        public override void Reset()
        {
            base.Reset();
            _inner.Reset();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot) => new GetInstanceFrame(this).Variable;
        
        public override Func<Scope, object> ToResolver(Scope topScope) => Resolve;

        public override object Resolve(Scope scope)
        {
            var instance = (T)_inner.Resolve(scope);
            var resolved = ResolvedInternal(instance, scope);
            return resolved;
        }

        public override object QuickResolve(Scope scope)
        {
            var instance = (T)_inner.QuickResolve(scope);
            var resolved = ResolvedInternal(instance, scope);
            return resolved;
        }

        private T ResolvedInternal(T instance, Scope scope)
        {
            return _interceptor.Intercept(ServiceType, instance, scope);
        }

        public override string ToString() => $"Intercepting instance for {_inner.ImplementationType.Name} for service {_inner.ServiceType.Name}";
    }
}
