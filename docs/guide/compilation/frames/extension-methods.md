# Extension Methods for Names in Code

To help generate code in memory, you'll want just a little bit of help from the following methods to
determine how a type should be written *in code* with these extension methods in `LamarCodeGeneration`:

1. `Type.NameInCode()` -- gives you the type name as it should appear in code. Handles inner types, generic types, well known simple types like `int`, and all other types

1. `Type.FullNameInCode()` -- gives you the type name as it should appear in code. Handles inner types, generic types, well known simple types like `int`, and all other types

The functionality of `NameInCode()` is demonstrated below with some of its xUnit tests:

```cs
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
```

Likewise, `FullNameInCode()` is shown below:

```cs
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
```
