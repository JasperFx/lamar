# Lamar and IDisposable

One of the main reasons to use an IoC container is to offload the work of disposing created objects at the right time in the application scope. Sure, it's something you should be aware of, but developers are less likely to make mistakes if that's just handled for them.

## Singletons

This one is easy, any object marked as the _Singleton_ lifecycle that implements `IDisposable` is disposed when the root container is
disposed:

<!-- snippet: sample_singleton-in-action -->
<a id='snippet-sample_singleton-in-action'></a>
```cs
[Fact]
public void singletons_are_disposed_when_the_container_is_disposed()
{
    var container = new Container(_ =>
    {
        _.ForSingletonOf<DisposableSingleton>();
    });

    // As a singleton-scoped object, every request for DisposableSingleton
    // will return the same object
    var singleton = container.GetInstance<DisposableSingleton>();
    singleton.ShouldBeSameAs(container.GetInstance<DisposableSingleton>());
    singleton.ShouldBeSameAs(container.GetInstance<DisposableSingleton>());
    singleton.ShouldBeSameAs(container.GetInstance<DisposableSingleton>());
    singleton.ShouldBeSameAs(container.GetInstance<DisposableSingleton>());

    singleton.WasDisposed.ShouldBeFalse();

    // now, dispose the Container
    container.Dispose();

    // the SingletonThing scoped object should be disposed
    singleton.WasDisposed.ShouldBeTrue();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/lifecycle_creation.cs#L22-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_singleton-in-action' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Nested Containers

As discussed in [nested containers](/guide/ioc/nested-containers), any transient or container-scoped object that implements `IDisposable` and is created
by a nested container will be disposed as the nested container is disposed:

<!-- snippet: sample_nested-disposal -->
<a id='snippet-sample_nested-disposal'></a>
```cs
[Fact]
public void nested_container_disposal()
{
    var container = new Container(_ =>
    {
        // A SingletonThing scoped service
        _.ForSingletonOf<IColorCache>().Use<ColorCache>();

        // A transient scoped service
        _.For<IColor>().Use<Green>();

        
        
        // An AlwaysUnique scoped service
        _.AddTransient<Purple>();

        _.AddTransient<Blue>();
    });

    ColorCache singleton = null;
    Green nestedGreen = null;
    Blue nestedBlue = null;
    Purple nestedPurple = null;

    using (var nested = container.GetNestedContainer())
    {
        // SingletonThing's are really built by the parent
        singleton = nested.GetInstance<IColorCache>()
            .ShouldBeOfType<ColorCache>();

        nestedGreen = nested.GetInstance<IColor>()
            .ShouldBeOfType<Green>();

        nestedBlue = nested.GetInstance<Blue>();

        nestedPurple = nested.GetInstance<Purple>();
    }

    // Transients created by the Nested Container
    // are disposed
    nestedGreen.WasDisposed.ShouldBeTrue();
    nestedBlue.WasDisposed.ShouldBeTrue();

    // Unique's created by the Nested Container
    // are disposed
    nestedPurple.WasDisposed.ShouldBeTrue();

    // NOT disposed because it's owned by
    // the parent container
    singleton.WasDisposed.ShouldBeFalse();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/nested_container.cs#L100-L153' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_nested-disposal' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_nested-disposal-1'></a>
```cs
[Fact]
public void nested_container_disposal()
{
    var container = new Container(_ =>
    {
        // A SingletonThing scoped service
        _.ForSingletonOf<IColorCache>().Use<ColorCache>();

        // A transient scoped service
        _.For<IColor>().Use<Green>();

        // An AlwaysUnique scoped service
        _.For<Purple>().AlwaysUnique();
    });

    ColorCache singleton = null;
    Green nestedGreen = null;
    Blue nestedBlue = null;
    Purple nestedPurple = null;

    using (var nested = container.GetNestedContainer())
    {
        // SingletonThing's are really built by the parent
        singleton = nested.GetInstance<IColorCache>()
            .ShouldBeOfType<ColorCache>();

        nestedGreen = nested.GetInstance<IColor>()
            .ShouldBeOfType<Green>();

        nestedBlue = nested.GetInstance<Blue>();

        nestedPurple = nested.GetInstance<Purple>();
    }

    // Transients created by the Nested Container
    // are disposed
    nestedGreen.WasDisposed.ShouldBeTrue();
    nestedBlue.WasDisposed.ShouldBeTrue();

    // Unique's created by the Nested Container
    // are disposed
    nestedPurple.WasDisposed.ShouldBeTrue();

    // NOT disposed because it's owned by
    // the parent container
    singleton.WasDisposed.ShouldBeFalse();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/nested_containers.cs#L121-L170' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_nested-disposal-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Transients

::: warning
This behavior is different from StructureMap. Be aware of this, or you may be vulnerable to memory leaks.
:::

Objects that implement `IDisposable` are tracked by the container that creates them and will be disposed whenever that container itself is disposed.
