using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen.Frames;

public class UseTests
{
    [Fact]
    public void find_by_type()
    {
        var uses = Use.Type<IWidget>();

        var expected = Variable.For<IWidget>();
        var variables = Substitute.For<IMethodVariables>();

        variables.FindVariable(typeof(IWidget)).Returns(expected);

        uses.FindVariable(variables).ShouldBe(expected);
    }

    [Fact]
    public void find_by_type_and_name()
    {
        var uses = Use.Type<IWidget>("w");

        var expected = Variable.For<IWidget>("w");
        var variables = Substitute.For<IMethodVariables>();

        variables.FindVariableByName(typeof(IWidget), "w").Returns(expected);

        uses.FindVariable(variables).ShouldBe(expected);
    }
}