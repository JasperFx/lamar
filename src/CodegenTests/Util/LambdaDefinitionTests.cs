using System;
using System.Linq.Expressions;
using CodegenTests.Codegen;
using JasperFx.CodeGeneration.Expressions;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Util;

public class LambdaDefinitionTests
{
    [Fact]
    public void register_a_variable_and_fetch_happy_path()
    {
        var definition = new LambdaDefinition();

        var variable = Variable.For<IWidget>();
        var expression = variable.ToVariableExpression(definition);

        definition.RegisterExpression(variable, expression);

        definition.ExpressionFor(variable).ShouldBeSameAs(expression);
    }

    [Fact]
    public void short_hand_register_variable()
    {
        var definition = new LambdaDefinition();

        var variable = Variable.For<IWidget>();

        definition.RegisterExpression(variable);

        definition.ExpressionFor(variable).ShouldNotBeNull();
    }

    [Fact]
    public void compile_simple()
    {
        var definition = new LambdaDefinition();
        var parameter = Expression.Parameter(typeof(string));
        definition.Arguments = new[] { parameter };
        definition.Body.Add(parameter);

        var func = definition.Compile<Func<string, string>>();

        func("hello").ShouldBe("hello");
    }
}