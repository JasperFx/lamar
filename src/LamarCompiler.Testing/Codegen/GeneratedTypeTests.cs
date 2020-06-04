using System.Linq;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class GeneratedTypeTests
    {

        [Fact]
        public void can_replace_base_ctor_argument_with_variable()
        {
            var type = new GeneratedType("SomeClass");
            type.InheritsFrom<ClassWithCtorArgs>();
            
            type.AllInjectedFields.Count.ShouldBe(3);
            type.BaseConstructorArguments.Length.ShouldBe(3);

            type.UseConstantForBaseCtor(Constant.ForEnum(Color.blue));
            
            type.AllInjectedFields.Count.ShouldBe(2);
            type.AllInjectedFields.Any(x => x.ArgType == typeof(Color))
                .ShouldBeFalse();
            
            type.BaseConstructorArguments[2].ShouldBeOfType<Variable>()
                .Usage.ShouldBe("LamarCompiler.Testing.Codegen.Color.blue");
        }
    }
    
    public abstract class ClassWithCtorArgs
    {
        public ClassWithCtorArgs(string name, int age, Color color)
        {
        }

        protected abstract void Go();
    }
}