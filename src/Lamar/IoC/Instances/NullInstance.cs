using System;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    public class NullInstance : Instance
    {
        public NullInstance(Type serviceType) : base(serviceType, serviceType, ServiceLifetime.Transient)
        {
            Hash = GetHashCode();
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => null;
        }

        public override object Resolve(Scope scope)
        {
            return null;
        }


        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new NullVariable(ServiceType);
        }
    }

    public class NullVariable : Variable
    {
        public NullVariable(Type variableType) : base(variableType, "null")
        {
        }

        public override void OverrideName(string variableName)
        {
            // nothing
        }
    }
}