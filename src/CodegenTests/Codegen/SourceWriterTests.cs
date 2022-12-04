using System.Linq;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class SourceWriterTests
{
    [Fact]
    public void end_block()
    {
        var writer = new SourceWriter();
        writer.Write("BLOCK:public void Go()");
        writer.Write("var x = 0;");
        writer.Write("END");

        var lines = writer.Code().ReadLines().ToArray();

        lines[3].ShouldBe("}");
    }

    [Fact]
    public void indention_within_a_block()
    {
        var writer = new SourceWriter();
        writer.Write("BLOCK:public void Go()");
        writer.Write("var x = 0;");

        var lines = writer.Code().ReadLines().ToArray();

        lines[2].ShouldBe("    var x = 0;");
    }

    [Fact]
    public void multi_end_blocks()
    {
        var writer = new SourceWriter();
        writer.Write("BLOCK:public void Go()");
        writer.Write("BLOCK:try");
        writer.Write("var x = 0;");
        writer.Write("END");
        writer.Write("END");

        var lines = writer.Code().ReadLines().ToArray();

        lines[5].ShouldBe("    }");

        // There's a line break between the blocks
        lines[7].ShouldBe("}");
    }

    [Fact]
    public void multi_level_indention()
    {
        var writer = new SourceWriter();
        writer.Write("BLOCK:public void Go()");
        writer.Write("BLOCK:try");
        writer.Write("var x = 0;");

        var lines = writer.Code().ReadLines().ToArray();

        lines[4].ShouldBe("        var x = 0;");
    }

    [Fact]
    public void write_block()
    {
        var writer = new SourceWriter();
        writer.Write("BLOCK:public void Go()");

        var lines = writer.Code().ReadLines().ToArray();

        lines[0].ShouldBe("public void Go()");
        lines[1].ShouldBe("{");
    }

    [Fact]
    public void write_using_by_type()
    {
        var writer = new SourceWriter();
        writer.UsingNamespace<ISourceWriter>();
        var lines = writer.Code().ReadLines().ToArray();

        lines[0].ShouldBe($"using {typeof(ISourceWriter).Namespace};");
    }

    [Fact]
    public void write_else()
    {
        var writer = new SourceWriter();
        writer.Write(@"
BLOCK:public void Go()
var x = 0;
");

        writer.WriteElse();
        var lines = writer.Code().Trim().ReadLines().ToArray();


        lines[3].ShouldBe("    else");
        lines[4].ShouldBe("    {");
    }

    [Fact]
    public void write_several_lines()
    {
        var writer = new SourceWriter();
        writer.Write(@"
BLOCK:public void Go()
var x = 0;
END
");

        var lines = writer.Code().Trim().ReadLines().ToArray();
        lines[0].ShouldBe("public void Go()");
        lines[1].ShouldBe("{");
        lines[2].ShouldBe("    var x = 0;");
        lines[3].ShouldBe("}");
    }

    [Fact]
    public void write_comment()
    {
        var writer = new SourceWriter();
        writer.Write("BLOCK:public void Go()");
        writer.WriteComment("Some Comment");

        var lines = writer.Code().ReadLines().ToArray();
        lines.Last().ShouldBe("    // Some Comment");
    }
}