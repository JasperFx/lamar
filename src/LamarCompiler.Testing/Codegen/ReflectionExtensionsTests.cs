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
        
        public class Envelope<T>
        {
            public T Value { get; set; }
            public Guid ExecutingUserId { get; set; }
        }

        public class Created
        {
            public Guid Id { get; set; }
        }

        public class Updated
        {
            public String UpdateValue { get; set; }
        }

        [Fact]
        public void short_name_of_generic_type()
        {
            var createdName = typeof(Envelope<Created>).ShortNameInCode();
            var updatedName = typeof(Envelope<Updated>).ShortNameInCode();
            
            createdName.ShouldBe("ReflectionExtensionsTests.Envelope<ReflectionExtensionsTests.Created>");
            createdName.ShouldNotBe(updatedName);
        }

        [Fact]
        public void short_name_of_open_generic_type()
        {
            var createdName = typeof(Envelope<>).ShortNameInCode();

            createdName.ShouldBe("ReflectionExtensionsTests.Envelope<>");
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
        
        #region sample_get-the-type-name-in-code
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
        #endregion
        
        #region sample_get-the-full-type-name-in-code
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
        #endregion
        
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

        [Fact]
        public void short_name_of_open_generic_types_nested_type()
        {
            var createdName = typeof(GenericTestClassWithNested<>.NestedTestClass).ShortNameInCode();

            createdName.ShouldBe("GenericTestClassWithNested<>.NestedTestClass");
        }

        [Fact]
        public void short_name_of_open_generic_types_nested_open_generic_type()
        {
            var createdName = typeof(GenericTestClassWithNested<>.NestedGenericTestClass<>).ShortNameInCode();

            createdName.ShouldBe("GenericTestClassWithNested<>.NestedGenericTestClass<>");
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

    public class GenericTestClassWithNested<TOuter>
    {
        public sealed class NestedTestClass
        {

        }

        public sealed class NestedGenericTestClass<TInner>
        {

        }
    }
}
