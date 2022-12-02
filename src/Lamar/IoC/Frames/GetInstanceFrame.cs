﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Frames
{
    
    #region sample_GetInstanceFrame
    public class GetInstanceFrame : SyncFrame, IResolverFrame
    {
        private static readonly MethodInfo _resolveMethod =
            ReflectionHelper.GetMethod<Instance>(x => x.Resolve(null));
        
        
        
        private Variable _scope;
        private readonly string _name;

        public GetInstanceFrame(Instance instance)
        {
            Variable = new ServiceVariable(instance, this, ServiceDeclaration.ServiceType);
            
            _name = instance.Name;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {_scope.Usage}.{nameof(Scope.GetInstance)}<{Variable.VariableType.FullNameInCode()}>(\"{_name}\");");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _scope = chain.FindVariable(typeof(Scope));
            yield return _scope;
        }
        
        public ServiceVariable Variable { get; }
        
        public void WriteExpressions(LambdaDefinition definition)
        {
            var scope = definition.Scope();
            var expr = definition.ExpressionFor(Variable);

            var instance = Variable.Instance;

            var @call = Expression.Call(Expression.Constant(instance), _resolveMethod, scope);
            var assign = Expression.Assign(expr, Expression.Convert(@call, Variable.VariableType));
            definition.Body.Add(assign);

            if (Next is null)
            {
                throw new InvalidCastException($"{typeof(GetInstanceFrame).GetFullName()}.{nameof(Next)} must not be null.");
            }
        }
    }
    #endregion
}