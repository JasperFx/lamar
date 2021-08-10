# Registration

Lamar provides a streamlined fluent interface called the _Registry DSL_ to configure a Lamar Container with both explicit registrations and conventional auto-registrations based on the older [StructureMap syntax](http://structuremap.github.io/registration/registry-dsl/). In addition, Lamar also directly supports the [ASP.Net Core DI style of service registrations](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1) ([IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=aspnetcore-2.1)) to ease the usage of Lamar with the entire .Net Core ecosystem.

The first step in using Lamar is configuring a `Container` object. The following examples are based on the usage of the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl).

Let's say that you have a simple set of services like this:

<!-- snippet: sample_foobar-model -->
<a id='snippet-sample_foobar-model'></a>
```cs
public interface IBar
{
}

public class Bar : IBar
{
}

public interface IFoo
{
}

public class Foo : IFoo
{
    public IBar Bar { get; private set; }

    public Foo(IBar bar)
    {
        Bar = bar;
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/Models.cs#L3-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_foobar-model' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

A simple configuration of a Lamar Container might then be:

<!-- snippet: sample_quickstart-configure-the-container -->
<a id='snippet-sample_quickstart-configure-the-container'></a>
```cs
// Example #1 - Create an container instance and directly pass in the configuration.
            var container1 = new Container(c =>
            {
                c.For<IFoo>().Use<Foo>();
                c.For<IBar>().Use<Bar>();
            });

// Example #2 - Create an container instance but add configuration later.
            var container2 = new Container();

            container2.Configure(c =>
            {
                c.For<IFoo>().Use<Foo>();
                c.For<IBar>().Use<Bar>();
            });
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/quickstart/configuring_the_container.cs#L19-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart-configure-the-container' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Initializing or configuring the container is usually done at application startup and is located as close as possible to the application's entry point. This place is sometimes referred to as the composition root of the application. In our example we are composing our application's object graph by connecting abstractions to concrete types.

We are using the fluent API `For<TInterface>().Use<TConcrete>()` which registers a default instance for a given plugin type (the TInterface type in this case). In our example we want an new instance of `Foo` every time we request the abstraction `IFoo`.

The recommended way of using the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) is by defining one or more `ServiceRegistry` classes. Typically, you would subclass the `ServiceRegistry` class, then use the Fluent API methods exposed by the `ServiceRegistry` class to describe a `Container` configuration. Here's a sample `ServiceRegistry` class used to configure the same types as in our previous example:

<!-- snippet: sample_foobar-registry -->
<a id='snippet-sample_foobar-registry'></a>
```cs
public class FooBarRegistry : ServiceRegistry
{
    public FooBarRegistry()
    {
        For<IFoo>().Use<Foo>();
        For<IBar>().Use<Bar>();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/Registries.cs#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_foobar-registry' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

When you set up a `Container` , you need to simply direct the `Container` to use the configuration in that `ServiceRegistry` class.

<!-- snippet: sample_quickstart-configure-the-container-using-a-registry -->
<a id='snippet-sample_quickstart-configure-the-container-using-a-registry'></a>
```cs
// Example #1
            var container1 = new Container(new FooBarRegistry());

// Example #2
            var container2 = new Container(c => { c.AddRegistry<FooBarRegistry>(); });

// Example #3 -- create a container for a single Registry
            var container3 = Container.For<FooBarRegistry>();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/quickstart/configuring_the_container.cs#L40-L49' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart-configure-the-container-using-a-registry' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip INFO
The Lamar team highly recommends using `ServiceRegistry` classes for your real application configuration.  The syntax is shorter inside the Registry class constructor than from within the `Container` constructor method. In addition, Registry classes can be used to modularize the configuration of a bigger application into more manageable chunks.  Lastly, using `ServiceRegistry` classes makes it easier to stand up additional `Container` objects in testing scenarios that reflect the real application composition.
:::

In real world applications you also have to deal with repetitive similar registrations. Such registrations are tedious, easy to forget and can be a weak spot in your application. Lamar provides [auto-registration and conventions](/guide/ioc/registration/auto-registration-and-conventions)  which mitigates this pain and eases the maintenance burden. Lamar exposes this feature through the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) by the `Scan` method.

In our example there is an reoccurring pattern, we are connecting the plugin type `ISomething` to a concrete type `Something`, meaning `IFoo` to `Foo` and `IBar` to `Bar`. Wouldn't it be cool if we could write a convention for exactly doing that? Fortunately Lamar has already one build in. Let's see how we can create an container with the same configuration as in the above examples.

<!-- snippet: sample_quickstart-configure-the-container-using-auto-registrations-and-conventions -->
<a id='snippet-sample_quickstart-configure-the-container-using-auto-registrations-and-conventions'></a>
```cs
// Example #1
            var container1 = new Container(c =>
                c.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                }));

// Example #2
            var container2 = new Container();

            container2.Configure(c =>
                c.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                }));
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/quickstart/configuring_the_container.cs#L54-L74' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart-configure-the-container-using-auto-registrations-and-conventions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

We instruct the scanner to scan through the calling assembly with default conventions on. This will find and register the default instance for `IFoo` and `IBar` which are obviously the concrete types `Foo` and `Bar`. Now whenever you add an additional interface `IMoreFoo` and a class `MoreFoo` to your application's code base, it's automatically picked up by the scanner.

Sometimes classes need to be supplied with some primitive value in its constructor. For example the `System.Data.SqlClient.SqlConnection` needs to be supplied with the connection string in its constructor. No problem, just set up the value of the constructor argument in the bootstrapping:

<!-- snippet: sample_quickstart-container-with-primitive-value -->
<a id='snippet-sample_quickstart-container-with-primitive-value'></a>
```cs
var container = new Container(c =>
{
    //just for demo purposes, normally you don't want to embed the connection string directly into code.
    c.For<IDbConnection>().Use<SqlConnection>().Ctor<string>().Is("YOUR_CONNECTION_STRING");
    //a better way would be providing a delegate that retrieves the value from your app config.    
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/quickstart/configuring_the_container.cs#L79-L86' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart-container-with-primitive-value' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

So far you have seen an couple of ways to work with the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) and configure a `Container` object. We have seen examples of configuration that allow us to build objects that don't depend on anything like the `Bar` class, or do depend on other types like the `Foo` class needs an instance of `IBar`. In our last example we have seen configuration for objects that need some primitive types like strings in its constructor function.
