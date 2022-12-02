using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public class InstanceConstructorFrame : ConstructorFrame, IResolverFrame
    {
        
        
        public InstanceConstructorFrame(ConstructorInstance instance, DisposeTracking disposal, Variable[] arguments,
            SetterArg[] setterParameters) : base(instance.ImplementationType, instance.Constructor, f => new ServiceVariable(instance, f, ServiceDeclaration.ServiceType))
        {
            Disposal = disposal;

            Parameters = arguments;
            
            switch (disposal)
            {
                case DisposeTracking.None:
                    Mode = ConstructorCallMode.Variable;
                    break;
                
                case DisposeTracking.WithUsing:
                    Mode = ConstructorCallMode.UsingNestedVariable;
                    break;
                
                case DisposeTracking.RegisterWithScope:
                    Mode = ConstructorCallMode.Variable;
                    var addDisposable = MethodCall.For<Scope>(s => s.TryAddDisposable(null));
                    addDisposable.Arguments[0] = Variable;
                    ActivatorFrames.Add(addDisposable);
                    break;
            }

            Setters.AddRange(setterParameters);
            
            
        }

        public DisposeTracking Disposal { get; set; }

        public void WriteExpressions(LambdaDefinition definition)
        {
            // No next, not disposable

            var isDisposed = BuiltType.CanBeCastTo<IDisposable>() || 
                             BuiltType.CanBeCastTo<IAsyncDisposable>();

            var callCtor = Expression.New(Ctor, Parameters.Select(definition.ExpressionFor));

            if (Next == null && !isDisposed && !Setters.Any())
            {
                definition.Body.Add(callCtor);
            }
            else
            {
                var variableExpr = Expression.Parameter(BuiltType, Variable.Usage);
                definition.RegisterExpression(Variable, variableExpr);
                definition.Assign(variableExpr, callCtor);

                foreach (var setter in Setters)
                {
                    var setMethod = BuiltType.GetProperty(setter.PropertyName).SetMethod;

                    var value = definition.ExpressionFor(setter.Variable);
                    var call = Expression.Call(variableExpr, setMethod, value);
                    definition.Body.Add(call);
                }

                if (isDisposed)
                {
                    definition.RegisterDisposable(variableExpr, Variable.VariableType);
                }

                if (Next == null)
                {
                    definition.Body.Add(definition.ExpressionFor(Variable));
                }
            }
        }
    }
}