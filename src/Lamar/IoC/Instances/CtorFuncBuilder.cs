using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lamar.IoC.Enumerables;
using Lamar.Scanning.Conventions;
using LamarCompiler;
using LamarCompiler.Util;


namespace Lamar.IoC.Instances
{
    [Obsolete("Think this can go away after moving to Expressions")]
    public static class CtorFuncBuilder
    {
        private static readonly Type[] _openTypes =
        {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>),
            typeof(Func<,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,,>)
        };

        private static readonly MethodInfo _coerceToList;
        private static readonly MethodInfo _coerceToArray;

        static CtorFuncBuilder()
        {
            _coerceToList = typeof(CtorFuncBuilder).GetMethod(nameof(CoerceToList));
            _coerceToArray = typeof(CtorFuncBuilder).GetMethod(nameof(CoerceArray));
        }

        public static IList<T> CoerceToList<T>(object elements)
        {
            return elements.As<object[]>().OfType<T>().ToList();
        }

        public static T[] CoerceArray<T>(object elements)
        {
            return elements.As<object[]>().OfType<T>().ToArray();
        }

        public static (Delegate func, Type funcType) LambdaTypeFor(Type concreteType, ConstructorInfo ctor)
        {
            return LambdaTypeFor(concreteType, concreteType, ctor);
        }

        public static (Delegate func, Type funcType) LambdaTypeFor(Type serviceType, Type concreteType,
            ConstructorInfo ctor)
        {
            var length = ctor.GetParameters().Length;
            var openType = _openTypes[length];


            var parameters = ctor.GetParameters();
            var arguments = new ParameterExpression[parameters.Length];
            var ctorParams = new Expression[parameters.Length];

            var parameterTypes = new List<Type>();

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                ctorParams[i] = arguments[i] = Expression.Parameter(parameter.ParameterType);
                parameterTypes.Add(parameter.ParameterType);
            }

            var parameterType = serviceType;
            parameterTypes.Add(parameterType);

            var funcType = openType.MakeGenericType(parameterTypes.ToArray());


            var callCtor = Expression.New(ctor, ctorParams);

            var lambda = Expression.Lambda(funcType, callCtor, arguments);

            const int FastExpressionCompilerParameterLimit = 8;
            if (parameterTypes.Count > FastExpressionCompilerParameterLimit)
            {
                var bigAssFunc = lambda.Compile();
                return (bigAssFunc, funcType);
            }

            var func = lambda.CompileFast();
            return (func, funcType);
        }
    }
}