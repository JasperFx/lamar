using System.Linq;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen.Model
{
    public class IfStyleTests
    {
        [Fact]
        public void open_and_close_as_if()
        {
            var writer = new SourceWriter();
            var style = IfStyle.If;
            
            style.Open(writer, "true");
            var lines = writer.Code().ReadLines().ToArray();
            
            lines[0].ShouldBe("if (true)");
            lines[1].ShouldBe("{");
            
            writer.IndentionLevel.ShouldBe(1);
        }
        
        [Fact]
        public void open_and_close_as_elseif()
        {
            var writer = new SourceWriter();
            var style = IfStyle.ElseIf;
            
            style.Open(writer, "true");
            var lines = writer.Code().ReadLines().ToArray();
            
            lines[0].ShouldBe("else if (true)");
            lines[1].ShouldBe("{");
            
            writer.IndentionLevel.ShouldBe(1);
        }

        [Fact]
        public void writes_the_closing_bracket()
        {
            var writer = new SourceWriter();
            var style = IfStyle.If;
            
            style.Open(writer, "true");
            style.Close(writer);
            var lines = writer.Code().ReadLines().ToArray();
            
            lines[2].ShouldBe("}");
            writer.IndentionLevel.ShouldBe(0);
        }
    }
}