# Try Getting an Optional Service by Plugin Type

::: tip INFO
The Lamar team does not recommend using "optional" dependencies as shown in this topic, but
external frameworks like ASP.Net MVC and Web API use this concept in their IoC container integration, so here it is. The Lamar team
prefers the usage of the [Nullo pattern](http://en.wikipedia.org/wiki/Null_Object_pattern) instead.
:::

In normal usage, if you ask Lamar for a service and Lamar doesn't recognize the requested type, the requested name, or know what the default should be for that type, Lamar will fail fast by throwing an exception rather than returning a null. Sometimes though, you may want to
retrieve an _optional_ service from Lamar that may or may not be registered in the Container. If that particular registration doesn't exist, you
just want a null value. Lamar provides first class support for _optional_ dependencies through the usage of the `IContainer.TryGetInstance()` methods.

::: tip INFO
In Lamar, the ASP.Net Core `IServiceProvider.GetService()` method has the same functionality and meaning as the `TryGetInstance()` method. If you
were wondering how Lamar's StructureMap-flavored `GetInstance()` method is different, that's how.
:::

Say you have a simple interface `IFoo` that may or may not be registered in the Container:

<!-- snippet: sample_optional-foo -->
<a id='snippet-sample_optional-foo'></a>
```cs
public interface IFoo
{
}

public class Foo : IFoo
{
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L8-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_optional-foo' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In your own code you might request the `IFoo` service like the code below, knowing that you'll
take responsibility yourself for building the `IFoo` service if Lamar doesn't have a registration
for `IFoo`:

<!-- snippet: sample_optional-real-usage -->
<a id='snippet-sample_optional-real-usage'></a>
```cs
public class MyFoo : IFoo
{
}

[Fact]
public void real_usage()
{
    var container = new Container();

    // if the container doesn't know about it,
    // I'll build it myself
    var foo = container.TryGetInstance<IFoo>()
              ?? new MyFoo();

}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L95-L112' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_optional-real-usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Just to make this perfectly clear, if Lamar has a default registration for `IFoo`, you get this behavior:

<!-- snippet: sample_optional-got-it -->
<a id='snippet-sample_optional-got-it'></a>
```cs
[Fact]
public void i_have_got_that()
{
    var container = new Container(_ => _.For<IFoo>().Use<Foo>());

    container.TryGetInstance<IFoo>()
        .ShouldNotBeNull();

    // -- or --

    container.TryGetInstance(typeof(IFoo))
        .ShouldNotBeNull();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L19-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_optional-got-it' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If Lamar knows nothing about `IFoo`, you get a null:

<!-- snippet: sample_optional-dont-got-it -->
<a id='snippet-sample_optional-dont-got-it'></a>
```cs
[Fact]
public void i_do_not_have_that()
{
    var container = new Container();

    container.TryGetInstance<IFoo>()
        .ShouldBeNull();

    // -- or --

    container.TryGetInstance(typeof(IFoo))
        .ShouldBeNull();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L36-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_optional-dont-got-it' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Concrete Types

Since it's not a perfect world, there are some gotchas you need to be aware of.
While Lamar will happily _auto-resolve_ concrete types that aren't registered,
that does not apply to the `TryGetInstance` mechanism:

<!-- snippet: sample_optional-no-concrete -->
<a id='snippet-sample_optional-no-concrete'></a>
```cs
public class ConcreteThing
{
}

[Fact]
public void no_auto_resolution_of_concrete_types()
{
    var container = new Container();

    container.TryGetInstance<ConcreteThing>()
        .ShouldBeNull();

    // now register ConcreteThing and do it again
    container.Configure(_ => { _.For<ConcreteThing>().Use<ConcreteThing>(); });

    container.TryGetInstance<ConcreteThing>()
        .ShouldNotBeNull();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L53-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_optional-no-concrete' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Optional Generic Types

If you are using open generic types, the `TryGetInstance()` mechanism **can** close the open generic registration
to satisfy the optional dependency like this sample:

<!-- snippet: sample_optional-close-generics -->
<a id='snippet-sample_optional-close-generics'></a>
```cs
public interface IThing<T>
{
}

public class Thing<T> : IThing<T>
{
}

[Fact]
public void can_try_get_open_type_resolution()
{
    var container = new Container(_ => { _.For(typeof(IThing<>)).Use(typeof(Thing<>)); });

    container.TryGetInstance<IThing<string>>()
        .ShouldBeOfType<Thing<string>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L75-L93' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_optional-close-generics' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
