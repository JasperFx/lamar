using System;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Lazy;

internal class FuncInstance<T> : Instance
{
    public FuncInstance() : base(typeof(Func<T>), typeof(Func<T>), ServiceLifetime.Transient)
    {
        Name = "func_of_" + typeof(T).FullNameInCode().Sanitize();
    }

    public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
    {
        return new GetFuncFrame(this, typeof(T)).Variable;
    }

    public override string WhyRequireServiceProvider(IMethodVariables method)
    {
        return "Func<T> uses Lamar scopes directly";
    }

    public override bool RequiresServiceProvider(IMethodVariables method)
    {
        return true;
    }

    public override Func<Scope, object> ToResolver(Scope topScope)
    {
        return scope =>
        {
            Func<T> func = scope.GetInstance<T>;

            return func;
        };
    }

    public override object Resolve(Scope scope)
    {
        Func<T> func = scope.GetInstance<T>;

        return func;
    }
}