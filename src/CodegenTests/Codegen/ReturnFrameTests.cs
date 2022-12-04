using System.Linq;
using JasperFx.CodeGeneration.Frames;
using JasperFx.RuntimeCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class ReturnFrameTests
{
    [Fact]
    public void simple_use_case_no_value()
    {
        var result = CodegenScenario.ForBaseOf<ISimpleAction>(m => m.Frames.Add(new ReturnFrame()));

        result.LinesOfCode.ShouldContain("return;");
    }

    [Fact]
    public void return_a_variable_by_type()
    {
        var result = CodegenScenario.ForBuilds<int, int>(m => m.Frames.Return(typeof(int)));

        result.LinesOfCode.ShouldContain("return arg1;");
        result.Object.Create(5).ShouldBe(5);
    }

    [Fact]
    public void return_explicit_variable()
    {
        var result = CodegenScenario.ForBuilds<int, int>(m =>
        {
            var arg = m.Arguments.Single();
            m.Frames.Return(arg);
        });

        result.LinesOfCode.ShouldContain("return arg1;");
        result.Object.Create(5).ShouldBe(5);
    }
}

public interface ISimpleAction
{
    void Go();
}