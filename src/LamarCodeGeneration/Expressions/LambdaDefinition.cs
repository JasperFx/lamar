using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration.Expressions
{
    public class LambdaDefinition
    {
        private readonly Dictionary<Variable, Expression> _variables = new Dictionary<Variable,Expression>();

        /// <summary>
        /// This could be anything that your IResolverFrame wants to depend on
        /// </summary>
        public object Context { get; set; }
        
        public void RegisterExpression(Variable variable, Expression expression)
        {
            if (_variables.ContainsKey(variable))
            {
                _variables[variable] = expression;
            }
            else
            {
                _variables.Add(variable, expression);
            }
        }
        
        public Expression RegisterExpression(Variable variable)
        {
            var expression = variable.ToVariableExpression(this);
            RegisterExpression(variable, expression);


            return expression;
        }

        public Expression ExpressionFor(Variable variable)
        {
            if (!_variables.TryGetValue(variable, out var expression))
            {
                expression = variable.ToVariableExpression(this);
                RegisterExpression(variable, expression);
            }
            
            return expression;
        }
        
        public ParameterExpression[] Arguments { get; set; } = new ParameterExpression[0];

        public TFunc Compile<TFunc>() where TFunc : class
        {
            if (!Body.Any()) throw new InvalidOperationException("There are no Body expressions for this LambdaDefinition");

            if (Body.Count == 1)
            {
                var lambda = Expression.Lambda<TFunc>(Body.Single(), Arguments);

                return lambda.CompileFast<TFunc>();
            }
            else
            {
                var variables = _variables.Values.OfType<ParameterExpression>().Where(x => x.Name != "scope");
                var body = Expression.Block(variables, Body);
                var lambda = Expression.Lambda<TFunc>(body, Arguments);

                return lambda.CompileFast<TFunc>();
            }
            

        }
        
        public IList<Expression> Body { get; } = new List<Expression>();

        public void Assign(Variable variable, Expression expression)
        {
            Body.Add(Expression.Assign(ExpressionFor(variable), expression));
        }
        
        public void Assign(ParameterExpression variable, Expression expression)
        {
            Body.Add(Expression.Assign(variable, expression));
        }


    }
}