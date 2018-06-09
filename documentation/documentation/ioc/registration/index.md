<!--Title: Registration-->
<!--Url: registration-->


Lamar provides a streamlined fluent interface called the _Registry DSL_ to configure a Lamar
Container with both explicit registrations and conventional auto-registrations based on the older [StructureMap syntax](http://structuremap.github.io/registration/registry-dsl/). In addition, Lamar also directly supports the [ASP.Net Core DI style
of service registrations](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1) ([IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=aspnetcore-2.1)) to ease the usage of Lamar with the entire .Net Core ecosystem.

The first step in using Lamar is configuring a <code>Container</code> object. The following examples are based on the usage of the <[linkto:documentation/ioc/registration/registry-dsl]>.

Let's say that you have a simple set of services like this:

<[sample:foobar-model]>

A simple configuration of a Lamar Container might then be:

<[sample:quickstart-configure-the-container]>

Initializing or configuring the container is usually done at application startup and is located as close as possible to the application's entry point. 
This place is sometimes referred to as the composition root of the application. 
In our example we are composing our application's object graph by connecting abstractions to concrete types.

We are using the fluent API `For<TInterface>().Use<TConcrete>()` which registers a default instance for a given plugin type (the TInterface type in this case). In our example we want an new instance of `Foo` every time we request the abstraction `IFoo`.

The recommended way of using the <[linkto:documentation/ioc/registration/registry-dsl]> is by defining one or more `ServiceRegistry` classes. Typically, you would subclass the `ServiceRegistry` class, 
then use the Fluent API methods exposed by the `ServiceRegistry` class to describe a `Container` configuration. 

Here's a sample `ServiceRegistry` class used to configure the same types as in our previous example:

<[sample:foobar-registry]>

When you set up a `Container` , you need to simply direct the `Container` to use the configuration in that `ServiceRegistry` class.

<[sample:quickstart-configure-the-container-using-a-registry]>

<[info]>
The Lamar team highly recommends using `ServiceRegistry` classes for your real application configuration.  The syntax is shorter inside the Registry class constructor than from within the `Container` constructor method. In addition, Registry classes can be used to modularize the configuration of a bigger application into more manageable chunks.  Lastly, using `ServiceRegistry` classes makes it easier to stand up additional `Container` objects in testing scenarios that reflect the real application composition.
<[/info]>


In real world applications you also have to deal with repetitive similar registrations. Such registrations are tedious, easy to forget and can be a weak spot in your application. Lamar provides <[linkto:documentation/ioc/registration/auto-registration-and-conventions]>  which mitigates this pain and eases the maintenance burden. Lamar exposes this feature through the <[linkto:documentation/ioc/registration/registry-dsl]> by the `Scan` method.

In our example there is an reoccuring pattern, we are connecting the plugin type `ISomething` to a concrete type `Something`, meaning `IFoo` to `Foo` and `IBar` to `Bar`. Wouldn't it be cool if we could write a convention for exactly doing that? Fortunatly Lamar has already one build in. Let's see how we can create an container with the same configuration as in the above examples.

<[sample:quickstart-configure-the-container-using-auto-registrations-and-conventions]>

We instruct the scanner to scan through the calling assembly with default conventions on. This will find and register the default instance for `IFoo` and `IBar` which are obviously the concrete types `Foo` and `Bar`. Now whenever you add an additional interface `IMoreFoo` and a class `MoreFoo` to your application's code base, it's automatically picked up by the scanner. 

Sometimes classes need to be supplied with some primitive value in its constructor. For example the `System.Data.SqlClient.SqlConnection` needs to be supplied with the connection string in its constructor. No problem, just set up the value of the constructor argument in the bootstrapping:

<[sample:quickstart-container-with-primitive-value]>

So far you have seen an couple of ways to work with the <[linkto:documentation/ioc/registration/registry-dsl]> and configure a `Container` object. We have seen examples of configuration that allow us to build objects that don't depend on anything like the `Bar` class, or do depend on other types like the `Foo` class needs an instance of `IBar`. In our last example we have seen configuration for objects that need some primitive types like strings in its constructor function.

