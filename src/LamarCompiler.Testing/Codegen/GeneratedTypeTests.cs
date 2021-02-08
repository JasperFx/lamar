using System.Diagnostics;
using System.Linq;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace LamarCompiler.Testing.Codegen
{
    public class GeneratedTypeTests
    {
        private readonly ITestOutputHelper _output;

        public GeneratedTypeTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
        
        public class SomeInnerClass{}

        [Fact]
        public void generate_code_with_base_type_that_is_generic()
        {

            var assembly = new GeneratedAssembly(new GenerationRules());
            var type = assembly.AddType("SomeClass", typeof(ClassWithGenericParameter<string>));
            
            assembly.CompileAll();
            
            
        }
        
        [Fact]
        public void generate_code_with_base_type_that_is_generic_using_an_inner_type_as_the_parameter()
        {

            var assembly = new GeneratedAssembly(new GenerationRules());
            var type = assembly.AddType("SomeClass", typeof(ClassWithGenericParameter<SomeInnerClass>));
            
            assembly.CompileAll();
            
            _output.WriteLine(type.SourceCode);
        }
    }

    public abstract class ClassWithGenericParameter<T>
    {
        public void Go()
        {
            
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