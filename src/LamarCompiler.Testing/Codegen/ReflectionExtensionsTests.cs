using System;
using LamarCodeGeneration;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen
{
    public class ReflectionExtensionsTests
    {
        [Fact]
        public void get_full_name_in_code_for_generic_type()
        {
            typeof(Handler<Message1>).FullNameInCode()
                .ShouldBe($"LamarCompiler.Testing.Codegen.Handler<{typeof(Message1).FullName}>");
        }

        public interface ISomeInterface<T>
        {
            
        }
        
        [Fact]
        public void get_full_name_in_code_for_inner_generic_type()
        {
            typeof(ISomeInterface<string>).FullNameInCode()
                .ShouldBe("LamarCompiler.Testing.Codegen.ReflectionExtensionsTests.ISomeInterface<string>");
        }
        
        [Fact]
        public void get_name_in_code_for_inner_generic_type()
        {
            typeof(ISomeInterface<string>).NameInCode()
                .ShouldBe("ReflectionExtensionsTests.ISomeInterface<string>");
        }
        
        // SAMPLE: get-the-type-name-in-code
        [Theory]
        [InlineData(typeof(void), "void")]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(long), "long")]
        [InlineData(typeof(bool), "bool")]
        [InlineData(typeof(double), "double")]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(Message1), "Message1")]
        [InlineData(typeof(Handler<Message1>), "Handler<LamarCompiler.Testing.Codegen.Message1>")]
        [InlineData(typeof(Handler<string>), "Handler<string>")]
        public void alias_name_of_task(Type type, string name)
        {
            // Gets the type name
            type.NameInCode().ShouldBe(name);
        }
        // ENDSAMPLE
        
        // SAMPLE: get-the-full-type-name-in-code
        [Theory]
        [InlineData(typeof(void), "void")]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(long), "long")]
        [InlineData(typeof(bool), "bool")]
        [InlineData(typeof(double), "double")]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(Message1), "LamarCompiler.Testing.Codegen.Message1")]
        [InlineData(typeof(Handler<Message1>), "LamarCompiler.Testing.Codegen.Handler<LamarCompiler.Testing.Codegen.Message1>")]
        [InlineData(typeof(Handler<string>), "LamarCompiler.Testing.Codegen.Handler<string>")]
        public void alias_full_name_of_task(Type type, string name)
        {
            type.FullNameInCode().ShouldBe(name);
        }
        // ENDSAMPLE
        
        [Fact]
        public void name_in_code_of_inner_type()
        {
            typeof(ThingHolder.Thing1).NameInCode().ShouldBe("ThingHolder.Thing1");
        }

        [Fact]
        public void full_name_in_code_of_generic_types_nested_type()
        {
            typeof(GenericTestClassWithNested<string>.NestedTestClass).FullNameInCode().ShouldBe("LamarCompiler.Testing.Codegen.GenericTestClassWithNested<string>.NestedTestClass");
        }
    }

    public class ThingHolder
    {
        public class Thing1
        {
            
        }
    }

    public class Handler<T>
    {

    }

    public class Message1{}

    public class GenericTestClassWithNested<T>
    {
        public sealed class NestedTestClass
        {

        }
    }
}
