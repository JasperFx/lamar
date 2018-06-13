using System;
using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    public class ReferencedInstance : Instance
    {
        private readonly string _instanceKey;
        private Instance _inner;

        public ReferencedInstance(Type serviceType, string instanceKey) : base(serviceType, serviceType, ServiceLifetime.Transient)
        {
            _instanceKey = instanceKey;
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return _inner.ToResolver(topScope);
        }

        public override object Resolve(Scope scope)
        {
            return _inner.Resolve(scope);
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return _inner.CreateVariable(mode, variables, isRoot);
        }

        public override object QuickResolve(Scope scope)
        {
            return _inner.QuickResolve(scope);
        }

        public override bool RequiresServiceProvider => _inner.RequiresServiceProvider;
        
        public override Variable CreateInlineVariable(ResolverVariables variables)
        {
            return _inner.CreateInlineVariable(variables);
        }

        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            _inner = services.FindInstance(ServiceType, _instanceKey);
            if (_inner == null) throw new InvalidOperationException($"Referenced instance of {ServiceType.FullNameInCode()} named '{_instanceKey}' does not exist");
            
            _inner.Parent = Parent;
            Lifetime = _inner.Lifetime;

            yield return _inner;
        }

        internal override string GetBuildPlan(Scope rootScope)
        {
            return _inner.GetBuildPlan(rootScope);
        }

        public override Instance CloseType(Type serviceType, Type[] templateTypes)
        {
            return _inner.CloseType(serviceType, templateTypes);
        }
    }
}