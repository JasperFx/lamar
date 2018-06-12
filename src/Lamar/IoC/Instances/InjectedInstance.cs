using System;
using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    public class InjectedInstance<T> : Instance
    {
        public InjectedInstance() : base(typeof(T), typeof(T), ServiceLifetime.Scoped)
        {
            Name = "Injected_" + DefaultArgName();
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => s.GetInjected<T>();
        }

        public override object Resolve(Scope scope)
        {
            return scope.GetInjected<T>();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new GetInjectedServiceFrame(this).Variable;
        }
        
        public class GetInjectedServiceFrame : SyncFrame
        {
            private Variable _scope;

            public GetInjectedServiceFrame(InjectedInstance<T> parent)
            {
                Variable = new ServiceVariable(parent, this);
            }
            
            public Variable Variable { get; }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                writer.Write($"var {Variable.Usage} = {_scope.Usage}.{nameof(Scope.GetInjected)}<{typeof(T).FullNameInCode()}>();");
                Next?.GenerateCode(method, writer);
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                _scope = chain.FindVariable(typeof(Scope));
                yield return _scope;
            }
        }
    }


}