using JasperFx.CodeGeneration.Model;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class SetterTests
{
    [Fact]
    public void default_setter()
    {
        var setter = new Setter(typeof(string), "Color");
        setter.Type.ShouldBe(SetterType.ReadWrite);

        setter.ToDeclaration().ShouldBe("public string Color {get; set;}");
    }

    [Fact]
    public void readonly_with_initial_value_of_variable()
    {
        var setter = Setter.ReadOnly("Color", Constant.ForString("red"));

        setter.ToDeclaration().ShouldBe("public string Color {get;} = \"red\";");
    }

    [Fact]
    public void staticreadonly_with_initial_value_of_variable()
    {
        var setter = Setter.StaticReadOnly("Color", Constant.ForString("red"));

        setter.ToDeclaration().ShouldBe("public static string Color {get;} = \"red\";");
    }

    [Fact]
    public void const_with_initial_value_of_variable()
    {
        var setter = Setter.Constant("Color", Constant.ForString("red"));

        setter.ToDeclaration().ShouldBe("public const string Color = \"red\";");
    }

    [Fact]
    public void const_with_initial_value_of_variable_using_enum()
    {
        var setter = Setter.Constant("Color", Constant.ForEnum(Color.red));

        setter.ToDeclaration()
            .ShouldBe(
                "public const CodegenTests.Codegen.Color Color = CodegenTests.Codegen.Color.red;");
    }
}

public enum Color
{
    red,
    blue,
    green
}