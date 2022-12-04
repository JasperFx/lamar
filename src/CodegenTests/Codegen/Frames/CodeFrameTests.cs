using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.RuntimeCompiler.Scenarios;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen.Frames;

public class CodeFrameTests
{
    [Fact]
    public void just_write_text()
    {
        var result = CodegenScenario.ForAction<int>(m => { m.Frames.Code("var x = 0;"); });

        result.LinesOfCode.ShouldContain("var x = 0;");
    }

    [Fact]
    public void write_text_with_substitution()
    {
        var result = CodegenScenario.ForAction<int>(m => { m.Frames.Code("var x = {0};", 22); });

        result.LinesOfCode.ShouldContain("var x = 22;");
    }

    [Fact]
    public void writes_with_source_writer_fanciness()
    {
        var result = CodegenScenario.ForAction<int>(m =>
        {
            m.Frames.Code(@"BLOCK:if (true)
// Comment
END
");
        });

        result.Code.ShouldContain("if");
        result.Code.ShouldNotContain("BLOCK");
    }

    [Fact]
    public void write_text_with_formatted_text()
    {
        var result = CodegenScenario.ForAction<int>(m => { m.Frames.Code("var x = {0};", "hi"); });

        result.LinesOfCode.ShouldContain("var x = \"hi\";");
    }

    [Fact]
    public void write_text_with_variables()
    {
        var result = CodegenScenario.ForAction<int>(m =>
        {
            var x = Variable.For<string>("x");
            m.Frames.Code("var {0} = {1};", x, "hi").Creates(x);
            m.Frames.Code("System.Console.WriteLine({0});", Use.Type<string>());
        });

        result.LinesOfCode.ShouldContain("var x = \"hi\";");
        result.LinesOfCode.ShouldContain("System.Console.WriteLine(x);");
    }
}