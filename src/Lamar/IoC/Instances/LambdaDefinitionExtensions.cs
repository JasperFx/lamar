using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.Core.Reflection;

namespace Lamar.IoC.Instances;

public static class LambdaDefinitionExtensions
{
    private static readonly MethodInfo _tryRegisterDisposable =
        ReflectionHelper.GetMethod<Scope>(x => x.TryAddDisposable(null));

    public static void RegisterDisposable(this LambdaDefinition definition, Expression parameter,
        Type variableType)
    {
        var scope = definition.Arguments.Single();

        var call = Expression.Call(scope, _tryRegisterDisposable, parameter);

        definition.Body.Add(call);
    }

    public static void TryRegisterDisposable(this LambdaDefinition definition, Expression parameter)
    {
        var scope = definition.Arguments.Single();

        var call = Expression.Call(scope, _tryRegisterDisposable, parameter);

        definition.Body.Add(call);
    }

    public static Expression Scope(this LambdaDefinition definition)
    {
        return definition.Arguments.Single();
    }
}