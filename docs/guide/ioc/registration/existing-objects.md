# Registering Existing Objects

It's frequently common to register existing objects with a Lamar `Container` and there are
overloads of the `ServiceRegistry.For().Use(object)` and `ServiceRegistry.For().Add(object)` methods to do just that:

<!-- snippet: sample_injecting-pre-built-object -->
<a id='snippet-sample_injecting-pre-built-object'></a>
```cs
[Fact]
public void should_be_able_to_resolve_from_the_generic_family_expression()
{
    var widget = new AWidget();
    var container = new Container(x => x.For(typeof(IWidget)).Use(widget).Named("mine"));

    container.GetInstance<IWidget>("mine").ShouldBeTheSameAs(widget);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Bugs/AddValueDirectlyWithGenericUsage.cs#L8-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_injecting-pre-built-object' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Injecting an existing object into the `Container` makes it a de facto singleton, but the `Container` treats it with a
special scope called `ObjectLifecycle` if you happen to look into the [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) diagnostics.

Lamar will attempt to call the `IDisposable.Dispose()` on any objects that are directly injected into a `Container`
that implement `IDisposable` when the `Container` itself is disposed.
