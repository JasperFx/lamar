using System;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Lazy
{
    internal class FuncByNameInstance<T> : Instance
    {
        public FuncByNameInstance() : base(typeof(Func<string, T>), typeof(Func<string, T>), ServiceLifetime.Transient)
        {
            Name = "func_by_name_of_" + typeof(T).NameInCode();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new GetFuncByNameFrame(this, typeof(T)).Variable;
        }
        

        public override bool RequiresServiceProvider(IMethodVariables method) => true;

        public override string WhyRequireServiceProvider(IMethodVariables method)
        {
            return "Func<string, T> uses Lamar scopes directly";
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return scope =>
            {
                T Func(string name) => scope.GetInstance<T>(name);

                return (Func<string, T>) Func;
            };
        }

        public override object Resolve(Scope scope)
        {
            T Func(string name) => scope.GetInstance<T>(name);

            return (Func<string, T>) Func;
        }
    }
}