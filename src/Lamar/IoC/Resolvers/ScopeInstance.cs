using System;
using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Resolvers
{

    public class RootScopeInstance<T> : Instance, IResolver
    {
        public RootScopeInstance() : base(typeof(T), typeof(T), ServiceLifetime.Singleton)
        {
            Name = typeof(T).Name;
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new CastRootScopeFrame(typeof(T)).Variable;
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => topScope;
        }

        public override object Resolve(Scope scope)
        {
            return scope.Root;
        }

        public override string ToString()
        {
            return $"Current {typeof(T).NameInCode()}";
        }
    }
    
    public class CastRootScopeFrame : SyncFrame
    {
        private Variable _scope;

        public CastRootScopeFrame(Type interfaceType)
        {
            Variable = new Variable(interfaceType, this);
        }
        
        public Variable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}) {_scope.Usage}.{nameof(Scope.Root)};");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _scope = chain.FindVariable(typeof(Scope));
            yield return _scope;
        }
    }
    
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