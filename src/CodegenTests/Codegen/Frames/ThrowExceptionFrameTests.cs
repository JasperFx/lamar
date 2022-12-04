using System;
using JasperFx.CodeGeneration.Frames;
using JasperFx.RuntimeCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen.Frames;

public class ThrowExceptionFrameTests
{
    [Fact]
    public void format_with_no_values()
    {
        ThrowExceptionFrame<NotImplementedException>.ToFormat(new object[0])
            .ShouldBe("throw new System.NotImplementedException();");
    }

    [Fact]
    public void format_with_one_value()
    {
        ThrowExceptionFrame<NotImplementedException>.ToFormat(new object[] { "boom" })
            .ShouldBe("throw new System.NotImplementedException({0});");
    }

    [Fact]
    public void format_with_multiple_values()
    {
        ThrowExceptionFrame<NotImplementedException>.ToFormat(new object[] { "boom", Use.Type<Exception>() })
            .ShouldBe("throw new System.NotImplementedException({0}, {1});");
    }

    [Fact]
    public void write_the_not_implemented_exception()
    {
        var results = CodegenScenario.ForAction<int>(x => { x.Frames.ThrowNotImplementedException(); });

        results.LinesOfCode.ShouldContain("throw new System.NotImplementedException();");
    }

    [Fact]
    public void write_the_not_supported_exception()
    {
        var results = CodegenScenario.ForAction<int>(x => { x.Frames.ThrowNotSupportedException(); });

        results.LinesOfCode.ShouldContain("throw new System.NotSupportedException();");
    }

    [Fact]
    public void write_with_arguments()
    {
        var results = CodegenScenario.ForAction<int>(x => { x.Frames.Throw<InvalidOperationException>("foo"); });

        results.LinesOfCode.ShouldContain("throw new System.InvalidOperationException(\"foo\");");
    }
}