using LamarCodeGeneration.Model;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
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
            var setter = Setter.ReadOnly("Color", new StringConstant("red"));

            setter.ToDeclaration().ShouldBe("public string Color {get;} = \"red\";");
        }
        
        [Fact]
        public void const_with_initial_value_of_variable()
        {
            var setter = Setter.Constant("Color", new StringConstant("red"));

            setter.ToDeclaration().ShouldBe("public const string Color = \"red\";");
        }
    }
}