using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public static class LambdaDefinitionExtensions
    {
        private static readonly MethodInfo _getDisposables =
            ReflectionHelper.GetProperty<Scope>(x => x.Disposables).GetGetMethod();

        private static readonly MethodInfo _add = ReflectionHelper.GetMethod<ConcurrentBag<IDisposable>>(x => x.Add(null));

        private static readonly MethodInfo _tryRegisterDisposable =
            ReflectionHelper.GetMethod<Scope>(x => x.TryAddDisposable(null));
        
        public static void RegisterDisposable(this LambdaDefinition definition, Expression parameter,
            Type variableType)
        {
            var scope = definition.Arguments.Single();
            var disposables = Expression.Call(scope, _getDisposables);

            
            var @call = Expression.Call(disposables, _add, variableType.CanBeCastTo<IDisposable>() ? parameter : Expression.Convert(parameter, typeof(IDisposable)));
            
            definition.Body.Add(@call);
        }
        
        public static void TryRegisterDisposable(this LambdaDefinition definition, Expression parameter)
        {
            var scope = definition.Arguments.Single();

            var @call = Expression.Call(scope, _tryRegisterDisposable, parameter);
            
            definition.Body.Add(@call);
        }

        public static Expression Scope(this LambdaDefinition definition)
        {
            return definition.Arguments.Single();
        }
    }
    
    
}