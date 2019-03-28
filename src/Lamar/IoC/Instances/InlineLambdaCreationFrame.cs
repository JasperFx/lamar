using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Lamar.IoC.Frames;
using LamarCompiler;
using LamarCompiler.Expressions;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Lamar.IoC.Instances
{
    public class InlineLambdaCreationFrame<TContainer> : SyncFrame, IResolverFrame
    {
        
        private Variable _scope;
        private readonly Setter _setter;
        

        public InlineLambdaCreationFrame(Setter setter, Instance instance)
        {
            Variable = new ServiceVariable(instance, this);
            _setter = setter;
        }
        
        public ServiceVariable Variable { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = ({Variable.VariableType.FullNameInCode()}){_setter.Usage}(({typeof(TContainer).FullNameInCode()}){_scope.Usage});");

            if(!Variable.VariableType.IsPrimitive && !Variable.VariableType.IsEnum && Variable.VariableType != typeof(string))
            {
                writer.WriteLine($"{_scope.Usage}.{nameof(Scope.TryAddDisposable)}({Variable.Usage});");
            }

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return _setter;
            _scope = chain.FindVariable(typeof(Scope));
            yield return _scope;
        }

        public void WriteExpressions(LambdaDefinition definition)
        {
            var scope = definition.Scope();
            var variableExpr = Expression.Variable(Variable.VariableType, Variable.Usage);
            definition.RegisterExpression(Variable, variableExpr);


            var invokeMethod = _setter.InitialValue.GetType().GetMethod("Invoke");
            var invoke = Expression.Call(Expression.Constant(_setter.InitialValue), invokeMethod, scope);
            
            definition.Body.Add(Expression.Assign(variableExpr, invoke));
            
            if (    !Variable.VariableType.IsValueType)
            {
                definition.TryRegisterDisposable(variableExpr);
            }
            

            if (Next is IResolverFrame next)
            {
                next.WriteExpressions(definition);
            }
            else
            {
                throw new InvalidCastException($"{Next.GetType().FullNameInCode()} does not implement {nameof(IResolverFrame)}");
            }
        }
    }
}