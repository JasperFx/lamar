---
title: Overriding Service Registrations
editLink: true
---

::: tip
There is the new [ConfigureTestServices()](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.testhost.webhostbuilderextensions.configuretestservices?view=aspnetcore-5.0) method
in ASP.Net Core 5 that purports to do the same thing, but the Lamar team believes that
the mechanism shown here will be more "correct" and also allows
you to use Lamar specific features.
:::

A new feature in Lamar v5.1 is a long requested way to reliably override service
registrations in .Net Core applications bootstrapped by the [generic host builder](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0).

Let's say that in test automation scenarios you'd like to override some of the services
in your normal application with testing stubs or to overwrite some kind of configuration. 
What you need to do is to take the `IHostBuilder` or `IWebHostBuilder` **just as it is
built by your application's bootstrapping**, but apply service registrations that
take precedence in Lamar over other registrations. That's a little trickier than
you might think because the `HostBuilder` / `WebHostBuilder` applies service registrations
in different places during bootstrapping, with registrations from `Startup.ConfigureServices()`
taking precedence in normal usage.

That's where the new `OverrideServices()` extension method comes into play. As an example,
let's say that in a test harness we want to just replace the normal ASP.Net Core `IServer`
service with a fake implementation called `FakeServer`. The following is the code to
do exactly that:

<!-- snippet: sample_usage_of_overrides -->
<a id='snippet-sample_usage_of_overrides'></a>
```cs
[Fact]
public void sample_usage_of_overrides()
{
    var builder = Program
        .CreateHostBuilder(Array.Empty<string>())

        // This is our chance to make service overrides
        .OverrideServices(s =>
        {
            s.For<IServer>().Use<FakeServer>();
        });

    using var host = builder.Build();

    host.Services.GetRequiredService<IServer>()
        .ShouldBeOfType<FakeServer>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.AspNetCoreTests/integration_with_aspnetcore.cs#L214-L234' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_usage_of_overrides' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In the code above, the lambda passed into the `OverrideServices()` method is executed in
the Lamar `Container` initialization **after all other other explicit registrations and policies
have been combined**. In effect, this means that any registrations -- or removal of registrations -- in
the `OverrideServices()` call are guaranteed to be processed last and reliably
override any original registrations.

The `OverrideServices()` extension methods are available for both `IHostBuilder` and `IWebHostBuilder`.
