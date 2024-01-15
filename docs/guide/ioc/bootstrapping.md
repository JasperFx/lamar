# Bootstrapping a Container

To configure and bootstrap a Lamar container, you have a couple options. You can create a `Container` object with inline registrations:

<!-- snippet: sample_bootstrap-inline -->
<a id='snippet-sample_bootstrap-inline'></a>
```cs
var container = new Container(x => { x.AddTransient<IClock, Clock>(); });
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/Bootstrapping.cs#L16-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrap-inline' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or pass in a configured `ServiceRegistry` object as shown below:

<!-- snippet: sample_bootstrap-with-registry -->
<a id='snippet-sample_bootstrap-with-registry'></a>
```cs
// Create a Lamar.ServiceRegistry object
// and define your service registrations
var registry = new ServiceRegistry();

// Use ASP.Net Core style registrations
// for basic functionality
registry.AddSingleton<IClock, Clock>();
registry.AddTransient<IWidget, RedWidget>();

// Or use StructureMap style registration syntax
// as an alternative or to use more advanced usage
registry.For<IClockFactory>()
    .Use<ClockFactory>()
    .Singleton();

var container = new Container(registry);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/Bootstrapping.cs#L25-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrap-with-registry' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Lamar's `ServiceRegistry` supports a subset of StructureMap's old `Registry` class and should be used as a replacement when replacing StructureMap with
Lamar. We renamed the class to disambiguate the name from the many other `Registry` classes in the CLR. `ServiceRegistry` implements the [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=aspnetcore-2.0) interface
from ASP.Net Core. You can also create a Lamar container by passing in an instance of `IServiceCollection` like you'd get within an ASP.Net Core application.
