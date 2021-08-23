using System;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Lazy
{
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
        

        public override bool RequiresServiceProvider { get; } = true;

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
}