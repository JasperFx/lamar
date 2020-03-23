using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;
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
    
    public class CastRootScopeFrame : SyncFrame, IResolverFrame
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

        public void WriteExpressions(LambdaDefinition definition)
        {
            var variableExpr = definition.RegisterExpression(Variable);
            definition.Body.Add(Expression.Assign(variableExpr, Expression.Convert(definition.ExpressionFor(_scope), Variable.VariableType)));
            
            Next?.As<IResolverFrame>().WriteExpressions(definition);
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