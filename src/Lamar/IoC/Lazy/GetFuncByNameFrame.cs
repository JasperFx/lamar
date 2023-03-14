using System;
using System.Linq.Expressions;
using System.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.CodeGeneration.Util;

namespace Lamar.IoC.Lazy
{
    internal class GetFuncByNameFrame : TemplateFrame, IResolverFrame
    {
        private static readonly MethodInfo _openMethod = typeof(Scope).GetMethod(nameof(Scope.FactoryByNameFor));
        
        private object _scope;
        private readonly Type _serviceType;

        public GetFuncByNameFrame(Instance instance, Type innerType)
        {
            _serviceType = innerType;
            Variable = new ServiceVariable(instance, this);
        }
        
        public Variable Variable { get; }

        protected override string Template()
        {
            _scope = Arg<Scope>();
            return $"System.Func<string, {_serviceType.FullNameInCode()}> {Variable.Usage} = name => {_scope}.{nameof(IContainer.GetInstance)}<{_serviceType.FullNameInCode()}>(name);";
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
        }
    }
}