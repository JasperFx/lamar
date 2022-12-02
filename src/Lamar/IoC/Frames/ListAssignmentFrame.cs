﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.IoC.Enumerables;
using Lamar.Scanning.Conventions;
using LamarCodeGeneration;
using LamarCodeGeneration.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Frames
{
    public class ListAssignmentFrame<T> : Frame, IResolverFrame
    {
        public ListAssignmentFrame(ListInstance<T> instance, Variable[] elements) : base(false)
        {
            ElementType = typeof(T);
            Variable = new ServiceVariable(instance, this);

            Elements = elements;
        }

        public Type ElementType { get; }

        public Variable[] Elements { get; }

        public Variable Variable { get; }
        public bool ReturnCreated { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var declaration = $"{typeof(List<>).Namespace}.List<{ElementType.FullNameInCode()}>";
            
            var elements = Elements.Select(x => x.Usage).Join(", ");
            if (ReturnCreated)
            {
                writer.Write($"return new {declaration}{{{elements}}};");
            }
            else
            {
                writer.Write($"var {Variable.Usage} = new {declaration}{{{elements}}};");
            }
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return Elements;
        }

        public void WriteExpressions(LambdaDefinition definition)
        {

            var listType = typeof(List<>).MakeGenericType(typeof(T));
            var ctor = listType.GetConstructors().Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.IsEnumerable());

            var initArray = Expression.NewArrayInit(ElementType, Elements.Select(definition.ExpressionFor));
            
            var expr = definition.ExpressionFor(Variable);
            

            var assign = Expression.Assign(expr, Expression.New(ctor, initArray));
            definition.Body.Add(assign);

            if (Next == null)
            {
                definition.Body.Add(expr);
            }
        }
    }
}
