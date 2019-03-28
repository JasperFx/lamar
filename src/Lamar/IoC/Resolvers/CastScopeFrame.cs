using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LamarCodeGeneration;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Resolvers
{
    public class CastScopeFrame : SyncFrame, IResolverFrame
    {
        private Variable _scope;

        public CastScopeFrame(Type interfaceType)
        {
            Variable = new Variable(interfaceType, this);
        }
        
        public Variable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}) {_scope.Usage};");
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
}