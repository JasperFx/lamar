using System;
using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Resolvers
{
    public class ScopeInstance<T> : Instance, IResolver
    {
        public ScopeInstance() : base(typeof(T), typeof(T), ServiceLifetime.Scoped)
        {
            Name = typeof(T).Name;
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new CastScopeFrame(typeof(T)).Variable;
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => s;
        }

        public override object Resolve(Scope scope)
        {
            return scope;
        }

        public override string ToString()
        {
            return $"Current {typeof(T).NameInCode()}";
        }
    }
}