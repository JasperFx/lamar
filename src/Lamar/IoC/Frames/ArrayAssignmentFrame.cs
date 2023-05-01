using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.IoC.Enumerables;

namespace Lamar.IoC.Frames;

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
    }

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
}