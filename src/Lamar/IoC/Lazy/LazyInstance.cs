using System;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Lazy
{
    public class LazyInstance<T> : Instance, IResolver
    {

        public LazyInstance() : base(typeof(Lazy<T>), typeof(Lazy<T>), ServiceLifetime.Transient)
        {
            Name = "lazy_of_" + typeof(T).NameInCode();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new GetLazyFrame(this, typeof(T)).Variable;
        }
        

        public override bool RequiresServiceProvider { get; } = true;

        public override object Resolve(Scope scope)
        {
            return new Lazy<T>(scope.GetInstance<T>);
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => new Lazy<T>(s.GetInstance<T>);
        }

    }
}