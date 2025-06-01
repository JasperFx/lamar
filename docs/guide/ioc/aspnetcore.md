# Integration with ASP.Net Core

::: tip INFO
As of Lamar 15.0, you can use the ASP.Net Core [FromKeyedServiceAttribute](https://andrewlock.net/exploring-the-dotnet-8-preview-keyed-services-dependency-injection-support/) as an equivalent to Lamar's older `NamedAttribute`
usage.
:::

To use Lamar within ASP.Net Core applications, also install the [Lamar.Microsoft.DependencyInjection](https://www.nuget.org/packages/Lamar.Microsoft.DependencyInjection/) library from NuGet to your ASP.Net Core project (and you can thank Microsoft for the clumsy naming convention, thank you).

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

::: warning
The `Startup.ConfigureServices(ServiceRegistry)` convention does not work as of ASP.Net Core 2.1. Use `ConfigureContainer(ServiceRegistry)` instead.
:::

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

## ASP.Net Core v3.*

The set up with ASP.Net Core v3 isn't really any different, but there's a known *gotcha* with the `AddControllers()` call as shown below:

<!-- snippet: sample_integration-with-mvc3 -->
<a id='snippet-sample_integration-with-mvc3'></a>
```cs
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            
            // Add Lamar
            .UseLamar()
            
            
            
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();

                webBuilder.ConfigureServices(services =>
                {
                    // This is important, the call to AddControllers()
                    // cannot be made before the usage of ConfigureWebHostDefaults
                    services.AddControllers();
                });
            });
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarWithAspNetCoreMvc3/Program.cs#L14-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_integration-with-mvc3' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To play it safe, add any registrations or configuration directly related to MVC Core directly within or after the call to `IHostBuilder.ConfigureWebHostDefaults()`. This is strictly an issue with ordering within MVC Core guts, and not particularly a problem with Lamar per se.

## Worker Service Core v3.*

To set up for a worker service, you'll use the same ConfigureContainer() albeit with a different signature.

<!-- snippet: sample_startup-worker-service -->
<a id='snippet-sample_startup-worker-service'></a>
```cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.UseLamar()
		.ConfigureServices((hostContext, services) =>
		{
			services.AddHostedService<Worker>();
		})
		.ConfigureContainer<Lamar.ServiceRegistry>((context, services) =>
		{
			// Also exposes Lamar specific registrations
			// and functionality
			services.Scan(s =>
			{
				s.TheCallingAssembly();
				s.WithDefaultConventions();
			});
		});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/SampleWorkerApp/Program.cs#L18-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_startup-worker-service' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Extended Command Line Diagnostics for ASP.Net Core

::: tip INFO
If you are targeting .Net Core 3.0 and/or `netstandard2.1`, use the newly consolidated [HostBuilder](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0) instead of the previous `IWebHostBuilder`.
:::

New with the Lamar 3.1.0 release is a separate Nuget package named *Lamar.Diagnostics* that can be used to add easy access to the [Lamar diagnostics](/guide/ioc/diagnostics/) for your ASP.Net Core application from the command line.

First, you need to be using the [Oakton.AspNetCore](https://jasperfx.github.io/oakton/documentation/aspnetcore/) package to execute commands in your ASP.Net Core application like this:

<!-- snippet: sample_using-oakton-aspnetcore -->
<a id='snippet-sample_using-oakton-aspnetcore'></a>
```cs
var registry = new ServiceRegistry();
registry.Scan(x =>
{
    x.Assembly(typeof(Program).Assembly);
    x.WithDefaultConventions();
});

var builder = new HostBuilder();

return builder
    // Replaces the built in DI container
    // with Lamar
    .UseLamar(registry)
    .ConfigureWebHostDefaults(x =>
    {
        // Normal ASP.Net Core bootstrapping
        x.UseUrls("http://localhost:5002")
            .UseKestrel()
            .UseStartup<Startup>();
    })

    .RunOaktonCommands(args);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/SampleWebApp/Program.cs#L22-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-oakton-aspnetcore' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Including this *Lamar.Diagnostics* NuGet into your ASP.Net Core application will add some additional Lamar diagnostic commands. If you open a command line tool to the root directory of your ASP.Net Core project
with the *Lamar.Diagnostics* and *Oakton.AspNetCore* NuGet installed and type the command for CLI usage `dotnet run -- ?` or `dotnet run -- help`, you'll get something like this:

```bash
Searching 'Lamar.Diagnostics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' for commands

  -----------------------------------------------------------------------------------
    Available commands:
  -----------------------------------------------------------------------------------
         check-env -> Execute all environment checks against the application
    lamar-scanning -> Runs Lamar's type scanning diagnostics
    lamar-services -> List all the registered Lamar services
    lamar-validate -> Runs all the Lamar container validations
               run -> Runs the configured AspNetCore application
  -----------------------------------------------------------------------------------
```

As you can see, there are three additional commands specific for the [built in Lamar diagnostics](/guide/ioc/diagnostics/). All of these commands support the [Oakton.AspNetCore](https://jasperfx.github.io/oakton/documentation/aspnetcore/) flags for overriding the ASP.Net Core hosting environment name, configuration settings, and default log level.

::: tip INFO
When all of these commands execute, they *do* build the underlying `IWebHost` for your application, but they do **not** start the Kestrel server or run any of your application `IHostedService` registrations. Your application will be
torn down and disposed as part of the command execution as well.
:::

## lamar-services

  This command displays the output of Lamar's [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) function against the underlying Lamar container of the configured ASP.Net Core application. The basic usage is `dotnet run -- lamar-services`.

The full usage is shown below:

```bash
 Usages for 'lamar-services' (List all the registered Lamar services)
  lamar-services [-f, --file <file>] [-n, --namespace <namespace>] [-a, --assembly <assembly>] [-t, --type <type>] [-b, --build-plans] [-e, --environment <environment>] [-v, --verbose] [-l, --log-level <logleve>] [----config:<prop> <value>]

  ----------------------------------------------------------------------------------------------------------------------------------------
    Flags
  ----------------------------------------------------------------------------------------------------------------------------------------
                  [-f, --file <file>] -> Optional file to write the results
        [-n, --namespace <namespace>] -> Optionally filter the results to only types in this namespace
          [-a, --assembly <assembly>] -> Optionally filter the results to only types in this assembly
                  [-t, --type <type>] -> Optionally filter the results to only this named type. Can be either a type name or a full name
                  [-b, --build-plans] -> Show the full build plans
    [-e, --environment <environment>] -> Use to override the ASP.Net Environment name
                      [-v, --verbose] -> Write out much more information at startup and enables console logging
          [-l, --log-level <logleve>] -> Override the log level
          [----config:<prop> <value>] -> Overwrite individual configuration items
  ----------------------------------------------------------------------------------------------------------------------------------------
```

The output can be filtered by using the `--namespace [namespace name]` or `--assembly [assembly name]` flags if you're only looking for certain registrations.
The `--type [type name]` flag can help you look for specific types. This flag can either match against a type's full name or just the type name and looks for both service types and implementation types.

You can write the results to a text file instead by specifying a file location with the `--file [file name]` flag.

Lastly, you can instead write out the [build plans]/guide/ioc/diagnostics/build-plans) with the same filtering options using the `--build-plans` flag.

## lamar-scanning

The `lamar-scanning` command gives you quick access to the [type scanning diagnostics](/guide/ioc/diagnostics/type-scanning) functionality in Lamar.

Using the command `dotnet run -- lamar-scanning` command on the sample application shown earlier in this page results in this output:

```bash
Searching 'Lamar.Diagnostics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' for commands
All Scanners
================================================================

Assemblies
----------
* SampleWebApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Default I[Name]/[Name] registration convention


No problems were encountered in exporting types from Assemblies
```

Any type scanning errors detected during application bootstrapping will be shown in the output. If there are any errors, this command will fail by returning a non-zero
return code. You could use this command as part of your continuous integration process to catch any kind of assembly loading problems found in type scanning.

## lamar-validate

The `lamar-validate` command will build your ASP.Net Core application (the `IWebHost`), then use Lamar's [built in container validation](/guide/ioc/diagnostics/validating-container-configuration) to verify
that the container can successfully build all known registrations. All validation errors will be reported, and the command will fail if there are any errors detected. This command can be used in continuous integration builds as another type of check on the system.

In its default usage, `dotnet run -- lamar-validate` will only validate the container configuration. If you use the `dotnet run -- lamar-validate Full` usage, the command will also execute any Lamar [environment tests](/guide/ioc/diagnostics/environment-tests).

## Adding Lamar Environment Checks

You can also add Lamar's container validation and its own environment tests to the *Oakton.AspNetCore* environment check functionality with the following usage of the `IServiceCollection.CheckLamarConfiguration()` extension method from *Lamar.Diagnostics* as shown below
in a sample `Startup.ConfigureContainer()` method:

<!-- snippet: sample_startup-with-check-lamar-configuration -->
<a id='snippet-sample_startup-with-check-lamar-configuration'></a>
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
        
        
        // This adds Lamar's validation to the 
        // Oakton.AspNetCore environment check support
        services.CheckLamarConfiguration();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/SampleWebApp/Startup.cs#L16-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_startup-with-check-lamar-configuration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, when you run the `dotnet run -- check-env` command for your application, you *should* see a check for the Lamar container:

```bash
Searching 'Lamar.Diagnostics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' for commands
Running Environment Checks
   1.) Success: Lamar IoC Service Registrations
All environment checks are good!

```
