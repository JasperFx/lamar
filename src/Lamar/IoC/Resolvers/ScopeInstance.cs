using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Resolvers;

public class RootScopeInstance<T> : Instance, IResolver
{
    public RootScopeInstance() : base(typeof(T), typeof(T), ServiceLifetime.Singleton)
    {
        Name = typeof(T).Name;
    }

    public override object Resolve(Scope scope)
    {
        return scope.Root;
    }

    public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
    {
        return new CastRootScopeFrame(typeof(T)).Variable;
    }

    public override Func<Scope, object> ToResolver(Scope topScope)
    {
        return s => topScope;
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

    public void WriteExpressions(LambdaDefinition definition)
    {
        var variableExpr = definition.RegisterExpression(Variable);

        var root = Expression.Property(definition.ExpressionFor(_scope), nameof(Scope.Root));
        var convert = Expression.Convert(root, Variable.VariableType);
        var assign = Expression.Assign(variableExpr, convert);

        definition.Body.Add(assign);
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write(
            $"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}) {_scope.Usage}.{nameof(Scope.Root)};");
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

    public override object Resolve(Scope scope)
    {
        return scope;
    }

    public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
    {
        return new CastScopeFrame(typeof(T)).Variable;
    }

    public override Func<Scope, object> ToResolver(Scope topScope)
    {
        return s => s;
    }

    public override string ToString()
    {
        return $"Current {typeof(T).NameInCode()}";
    }
}