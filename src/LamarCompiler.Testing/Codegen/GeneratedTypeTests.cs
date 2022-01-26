using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
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
        public void write_comment()
        {
            var type = new GeneratedType("SomeClass");
            type.CommentType("some comment text");
            
            type.Header.ShouldBeOfType<OneLineComment>()
                .Text.ShouldBe("some comment text");
        }

        [Fact]
        public void write_comment_text_into_source_code()
        {
            var assembly = new GeneratedAssembly(new GenerationRules());
            var type = assembly.AddType("SomeClass", typeof(ClassWithGenericParameter<SomeInnerClass>));
            type.CommentType("Hey, look at this!");
            
            assembly.CompileAll();
            
            type.SourceCode.ReadLines()
                .ShouldContain("    // Hey, look at this!");
            _output.WriteLine(type.SourceCode);
        }
        
        [Fact]
        public void write_footer_into_source_code()
        {
            var assembly = new GeneratedAssembly(new GenerationRules());
            var type = assembly.AddType("SomeClass", typeof(ClassWithGenericParameter<SomeInnerClass>));
            type.Footer = new OneLineComment("Hey, look at this!");
            
            assembly.CompileAll();
            
            type.SourceCode.ReadLines()
                .ShouldContain("    // Hey, look at this!");
            _output.WriteLine(type.SourceCode);
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

        [Fact]
        public void can_use_Task_as_the_class_name()
        {
            var assembly = new GeneratedAssembly(new GenerationRules());
            var type = assembly.AddType("Task", typeof(Thing));
            var method = type.MethodFor("Do").Frames.Code("// stuff");
            
            assembly.CompileAll();
            
            _output.WriteLine(type.SourceCode);
        }
    }

    public abstract class Thing
    {
        public abstract System.Threading.Tasks.Task Do();
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