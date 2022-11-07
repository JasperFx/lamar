using System;
using System.Linq.Expressions;
using System.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Lazy
{
    internal class GetLazyFrame : TemplateFrame, IResolverFrame
    {
        
        private static readonly MethodInfo _openMethod = typeof(Scope).GetMethod(nameof(Scope.LazyFor));

        private object _scope;
        private readonly Type _serviceType;

        public GetLazyFrame(Instance instance, Type innerType)
        {
            _serviceType = innerType;
            Variable = new ServiceVariable(instance, this);
        }
        
        public Variable Variable { get; }

        protected override string Template()
        {
            _scope = Arg<Scope>();
            return $"var {Variable.Usage} = new System.Lazy<{_serviceType.FullNameInCode()}>(() => {_scope}.{nameof(IContainer.GetInstance)}<{_serviceType.FullNameInCode()}>());";
        }
        
        public void WriteExpressions(LambdaDefinition definition)
        {
            var scope = definition.Scope();
            var closedMethod = _openMethod.MakeGenericMethod(_serviceType);
            var expr = definition.ExpressionFor(Variable);

            var call = Expression.Call(scope, closedMethod);
            var assign = Expression.Assign(expr, call);
            
            definition.Body.Add(assign);
            
            
            if (Next == null)
            {
                definition.Body.Add(expr);
            }
            else
            {
                Next.As<IResolverFrame>().WriteExpressions(definition);
            }
        }
    }
}