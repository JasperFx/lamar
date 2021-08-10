# Getting Started

This page does assume that you are already familiar with IoC containers. For more background on the concepts
and usage of an IoC container within your application, see [concepts](/guide/ioc/concepts)

Lamar is a .NET library that provides two pieces of functionality:

1. A fast [Inversion of Control Container](https://www.martinfowler.com/articles/injection.html) that natively supports the [ASP.Net Core DI abstractions](https://code.msdn.microsoft.com/Dependency-injection-in-f789ceaa) and a subset of the older [StructureMap library](https://structuremap.github.io)
1. The dynamic code generation and compilation features used underneath the IoC implementation

## Lamar as IoC Container

To get started, just add [Lamar](https://www.nuget.org/packages/Lamar/) to your project through Nuget.

Most of the time you use an IoC container these days, it's probably mostly hidden inside of some kind of application framework. However, if you wanted to use Lamar all by itself you would first [bootstrap a Lamar container](/guide/ioc/bootstrapping) with all its service registrations something like this:

<[sample:start-a-container]>

Now, to resolve services from your container:

<[sample:resolving-services-quickstart]>

Definitely note that the old StructureMap style of service resolution is semantically different than ASP.Net Core's DI resolution methods. That's been the cause of much user aggravation over the years.

## Lamar within ASP&period;Net Core Applications

To use Lamar within ASP.Net Core applications, also install the [Lamar.Microsoft.DependencyInjection](https://www.nuget.org/packages/Lamar.Microsoft.DependencyInjection/) library from Nuget to your ASP.Net Core project (and you can thank Microsoft for the clumsy naming convention, thank you).

With that NuGet installed, your normal ASP.Net Core bootstrapping changes just slightly. When you bootstrap your `IWebHostBuilder` object
that configures ASP.Net Core, you also need to call the `UseLamar()` method as shown below:

<[sample:getting-started-main]>

If you use a `StartUp` class for extra configuration, your `ConfigureContainer()` method *can* take in a `ServiceRegistry` object from Lamar for service registrations in place of the ASP.Net Core `IServiceCollection` interface as shown below:

<[sample:getting-started-startup]>

You can also still write `ConfigureServices(IServiceCollection)`, but you'd miss out on most of Lamar's extra functionality beyond what that abstraction
provides.

And that is that, you're ready to run your ASP.Net Core application with Lamar handling service resolution and object cleanup during your
HTTP requests.

## Lamar for Runtime Code Generation & Compilation

Please see [compilation](/guide/compilation/) for more information.
