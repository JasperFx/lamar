# Getting Started

This page does assume that you are already familiar with IoC containers. For more background on the concepts
and usage of an IoC container within your application, see [concepts](/guide/ioc/concepts)

## What is Lamar?

Lamar is a .NET library that provides two pieces of functionality:

1. A fast [Inversion of Control Container](https://www.martinfowler.com/articles/injection.html) that natively supports the [ASP.Net Core DI abstractions](https://code.msdn.microsoft.com/Dependency-injection-in-f789ceaa) and a subset of the older [StructureMap library](https://structuremap.github.io)
1. The dynamic code generation and compilation features used underneath the IoC implementation

## History and Motivation

[StructureMap](https://structuremap.github.io) was the first production capable Inversion of Control container
in the .Net ecosystem, with its first production usage in the summer of 2004. Despite its success,
StructureMap's internals were not keeping up well with modern usage within ASP.Net Core applications and 
lagged in performance. Lamar was conceived as a replacement for StructureMap that would hugely improve
upon StructureMap's performance, be completely compliant with the new ASP.Net Core DI behavior,
and provide an easy off ramp for existing StructureMap users.


## Lamar as IoC Container

To get started, just add [Lamar](https://www.nuget.org/packages/Lamar/) to your project through Nuget.

Most of the time you use an IoC container these days, it's probably mostly hidden inside of some kind of application framework. However, if you wanted to use Lamar all by itself you would first [bootstrap a Lamar container](/guide/ioc/bootstrapping) with all its service registrations something like this:

<!-- snippet: sample_start-a-container -->
<a id='snippet-sample_start-a-container'></a>
```cs
var container = new Container(x =>
{
    // Using StructureMap style registrations
    x.For<IClock>().Use<Clock>();
    
    // Using ASP.Net Core DI style registrations
    x.AddTransient<IClock, Clock>();
    
    // and lots more services in all likelihood
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/GettingStarted.cs#L11-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_start-a-container' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, to resolve services from your container:

<!-- snippet: sample_resolving-services-quickstart -->
<a id='snippet-sample_resolving-services-quickstart'></a>
```cs
// StructureMap style

// Get a required service
var clock = container.GetInstance<IClock>();

// Try to resolve a service if it's registered
var service = container.TryGetInstance<IService>();

// ASP.Net Core style
var provider = (IServiceProvider)container;

// Get a required service
var clock2 = provider.GetRequiredService<IClock>();

// Try to resolve a service if it's registered
var service2 = provider.GetService<IService>();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/GettingStarted.cs#L24-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_resolving-services-quickstart' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Definitely note that the old StructureMap style of service resolution is semantically different than ASP.Net Core's DI resolution methods. That's been the cause of much user aggravation over the years.

## Lamar within ASP.Net Core Applications

To use Lamar within ASP.Net Core applications, also install the [Lamar.Microsoft.DependencyInjection](https://www.nuget.org/packages/Lamar.Microsoft.DependencyInjection/) library from Nuget to your ASP.Net Core project (and you can thank Microsoft for the clumsy naming convention, thank you).

With that NuGet installed, your normal ASP.Net Core bootstrapping changes just slightly. When you bootstrap your `IWebHostBuilder` object
that configures ASP.Net Core, you also need to call the `UseLamar()` method as shown below:

<!-- snippet: sample_getting-started-main -->
<a id='snippet-sample_getting-started-main'></a>
```cs
public static void Main(string[] args)
{
    var builder = new WebHostBuilder();
    builder
        // Replaces the built in DI container
        // with Lamar
        .UseLamar()
        
        // Normal ASP.Net Core bootstrapping
        .UseUrls("http://localhost:5002")
        .UseKestrel()
        .UseStartup<Startup>();

    builder.Start();

}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.AspNetCoreTests/Samples/StartUp.cs#L14-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getting-started-main' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If you use a `StartUp` class for extra configuration, your `ConfigureContainer()` method *can* take in a `ServiceRegistry` object from Lamar for service registrations in place of the ASP.Net Core `IServiceCollection` interface as shown below:

<!-- snippet: sample_getting-started-startup -->
<a id='snippet-sample_getting-started-startup'></a>
```cs
public class Startup
{
    // Take in Lamar's ServiceRegistry instead of IServiceCollection
    // as your argument, but fear not, it implements IServiceCollection
    // as well
    public void ConfigureContainer(ServiceRegistry services)
    {
        // Supports ASP.Net Core DI abstractions
        services.AddMvc();
        services.AddLogging();
        
        // Also exposes Lamar specific registrations
        // and functionality
        services.Scan(s =>
        {
            s.TheCallingAssembly();
            s.WithDefaultConventions();
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.AspNetCoreTests/Samples/StartUp.cs#L35-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getting-started-startup' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can also still write `ConfigureServices(IServiceCollection)`, but you'd miss out on most of Lamar's extra functionality beyond what that abstraction
provides.

And that is that, you're ready to run your ASP.Net Core application with Lamar handling service resolution and object cleanup during your
HTTP requests.

## Lamar with ASP.NET Core Minimal Hosting

Minimal hosting provides you with a condensed programming experience, only exposing the minimum required to get an ASP.NET Core application running. You can still use Lamar with the minimal hosting approach, but you will be required to take a few additional steps to wire the Lamar container correctly into the ASP.NET Core infrastructure. Follow the example below. You will still need the NuGet packages mentioned in the previous section.

```c#
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Step 1. Add Lamar as the Host Container
builder.Host.UseLamar();

// Step 2. Add Lamar and registrations to ASP.NET Core 
builder.Services.AddLamar(ServiceRegistry.For(registry => {
    registry.AddScoped<IService, MyService>();
}));

var app = builder.Build();

// Step 3. Use [FromServices] Attribute
// Note: Blows up with "inferred" exception without it

app.MapGet("/", ([FromServices] IService service) => service.Hi());
app.Run();

public class MyService : IService {
    public string Hi() {
        return "Hello, World!";
    }
}

public interface IService {
    string Hi();
}
```

**Note: It's important that services injected into minimal endpoints have a `[FromServices]` attribute.**

## Lamar for Runtime Code Generation & Compilation

Please see [compilation](/guide/compilation/) for more information.
