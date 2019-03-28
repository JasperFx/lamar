using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lamar.IoC.Enumerables;
using Lamar.Scanning.Conventions;
using LamarCodeGeneration;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCompiler;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Frames
{
    public class ArrayAssignmentFrame<T> : SyncFrame, IResolverFrame
    {
        public ArrayAssignmentFrame(ArrayInstance<T> instance, Variable[] elements)
        {
            Elements = elements;
            Variable = new ServiceVariable(instance, this);

            ElementType = typeof(T);
        }



        public Type ElementType { get; }

        public Variable[] Elements { get; }

        public Variable Variable { get; }
        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var elements = Elements.Select(x => x.Usage).Join(", ");

            var arrayType = ElementType.FullNameInCode();

            if (ReturnCreated)
            {
                writer.Write($"return new {arrayType}[]{{{elements}}};");
            }
            else
            {
                writer.Write($"var {Variable.Usage} = new {arrayType}[]{{{elements}}};");
            }
            
            
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return Elements;
        }

        public void WriteExpressions(LambdaDefinition definition)
        {
            var init = Expression.NewArrayInit(ElementType, Elements.Select(definition.ExpressionFor));
            var expr = definition.ExpressionFor(Variable);

            var assign = Expression.Assign(expr, init);
            definition.Body.Add(assign);

            if (Next == null)
            {
                definition.Body.Add(expr);
            }
            else if (Next is IResolverFrame next)
            {
                next.WriteExpressions(definition);
            }
            else
            {
                throw new InvalidCastException($"{Next.GetType().GetFullName()} does not implement {nameof(IResolverFrame)}");
            }
        }
    }
}
