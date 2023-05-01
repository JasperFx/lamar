using System;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Lazy;

internal class LazyInstance<T> : Instance, IResolver
{
    public LazyInstance() : base(typeof(Lazy<T>), typeof(Lazy<T>), ServiceLifetime.Transient)
    {
        Name = "lazy_of_" + typeof(T).NameInCode();
    }

    public override object Resolve(Scope scope)
    {
        return new Lazy<T>(scope.GetInstance<T>);
    }

    public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
    {
        return new GetLazyFrame(this, typeof(T)).Variable;
    }


    public override bool RequiresServiceProvider(IMethodVariables method)
    {
        return true;
    }

    public override string WhyRequireServiceProvider(IMethodVariables method)
    {
        return "Lazy<T> uses Lamar scopes directly";
    }

    public override Func<Scope, object> ToResolver(Scope topScope)
    {
        return s => new Lazy<T>(s.GetInstance<T>);
    }
}