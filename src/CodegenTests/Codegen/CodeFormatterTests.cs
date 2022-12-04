using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public enum Numbers
{
    one,
    two
}

public class CodeFormatterTests
{
    [Fact]
    public void write_string()
    {
        CodeFormatter.Write("Hello!")
            .ShouldBe("\"Hello!\"");
    }

    [Fact]
    public void write_enum()
    {
        CodeFormatter.Write(Numbers.one)
            .ShouldBe("CodegenTests.Codegen.Numbers.one");
    }

    [Fact]
    public void write_type()
    {
        CodeFormatter.Write(GetType())
            .ShouldBe($"typeof({GetType().FullNameInCode()})");
    }

    [Fact]
    public void write_null()
    {
        CodeFormatter.Write(null).ShouldBe("null");
    }

    [Fact]
    public void write_variable()
    {
        var variable = Variable.For<int>("number");

        CodeFormatter.Write(variable).ShouldBe(variable.Usage);
    }
}