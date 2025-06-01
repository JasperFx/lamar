# Lamar Diagnostics

Like StructureMap before it, one of Lamar's big differentiators from other IoC tools is its strong support for built in diagnostic tools.

::: tip
Lamar 15.0 utilizes the [JasperFx library](https://github.com/jasperfx/jasperfx) internally that comes with its own command line execution engine, and the former
Lamar.Diagnostics Nuget has been completely merged into Lamar itself.
:::

<!-- snippet: sample_using-lamar-diagnostics -->
<a id='snippet-sample_using-lamar-diagnostics'></a>
```cs
static Task<int> Main(string[] args)
{
    // Start up your HostBuilder as normal
    return new HostBuilder()
        .UseLamar((context, services) =>
        {
            // This adds a Container validation
            // to the Oakton "check-env" command
            services.CheckLamarConfiguration();

            // And the rest of your application's
            // DI registrations.
            services.IncludeRegistry<TestClassRegistry>();

            // This one was problematic with oddball type names,
            // so it's in our testing
            services.AddHttpClient();
        })

        // Call this method to start your application
        // with JasperFx handling the command line parsing
        // and delegation
        // This will be included with your reference to Lamar,
        // no other Nugets are necessary!
        .RunJasperFxCommands(args);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarDiagnosticsWithNetCore3Demonstrator/Program.cs#L15-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-lamar-diagnostics' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Once the `Lamar.Diagnostics` NuGet is installed to your application and you've opted into Oakton to handle command line options, typing this command at the root of your project will show all the installed commands:

```bash
dotnet run -- help
```

If `Lamar.Diagnostics` is installed, you should see three lamar related commands as shown below:

```bash
---------------------------------------------------------------------------------------------
  Available commands:
---------------------------------------------------------------------------------------------
        check-env -> Execute all environment checks against the application
        describe -> Writes out a description of your running application...
            help -> list all the available commands
  lamar-scanning -> Runs Lamar's type scanning diagnostics
  lamar-services -> List all the registered Lamar services
  lamar-validate -> Runs all the Lamar container validations
              run -> Runs the configured AspNetCore application
---------------------------------------------------------------------------------------------
```

::: tip INFO
All the diagnostic commands expose an `-e` flag to control the host environment name of the running application.
:::

## Validating the Container Configuration

To validate the Lamar configuration of your system, use this command from the root of your project:

```bash
dotnet run -- lamar-validate
```

Running that command is going to:

1. Bootstrap your application which sets up the Lamar container
1. Checks that every service registration can find all necessary dependencies
1. If running in the default "full" mode, tries to build all known registrations one by one
1. If running in the default "full" mode, executes all the [Lamar environment checks](/guide/ioc/diagnostics/environment-tests) in your system
1. Write out the stack traces of any and all exceptions that Lamar encounters

If everything checks out, you'll get this friendly output in green:

```bash
Lamar registrations are all good!
```

Otherwise, you're going to get a whole lot of .Net exception stack traces with explanatory text about what registrations or environment tests failed.

The command itself will return a non zero exit code, so if `lamar-validate` is used within your automated build, it will fail your build if the validation fails **by design**.

To run a faster check of only the configuration, use the flag shown below:

```bash
dotnet run -- lamar-validate ConfigOnly
```

## Analyzing the Lamar Configuration

The old Lamar/StructureMap *WhatDoIHave()* diagnostic report is available from the command line in an enhanced command like this:

```bash
dotnet run -- lamar-services
```

This command makes heavy usage of the [Spectre.Console](https://spectresystems.github.io/spectre.console) library to format the output in a much more readable way than the old, purely textual version.

The output is somewhat ellided to eliminate some of the noise registrations added by `HostBuilder` like `IOptions<T>` or `ILogger<T>`. To see **everything**, use the verbose flag:

```bash
dotnet run -- lamar-services --verbose
```

::: tip INFO
All service registration filtering is done by the **service type** rather than the **implementation type**
:::

To filter the results to zero in on specific type registrations, you can filter by assembly:

```bash
dotnet run -- lamar-services --assembly YourAssemblyName
```

or by namespace (and this is inclusive):

```bash
dotnet run -- lamar-services --namespace NamespaceName
```

or by a specific type name:

```bash
dotnet run -- lamar-services --type YourTypeName
```

The filtering by type name is case insensitive, and looks for type names that contain your filter. So looking for `--type options` will find every possible registration of `IOptions<T>` for example.

To get more detailed information about exactly how Lamar is building these service registrations, use this option:

```bash
dotnet run -- lamar-services --build-plans
```

Lastly, you can save off the output of the `lamar-services` command to either a text file:

```bash
dotnet run -- lamar-services --build-plans --file services.txt
```

or keep the formatting in HTML by naming the file with either an `htm` or `html` file extension:

```bash
dotnet run -- lamar-services --build-plans --file services.htm
```

## Type Scanning Diagnostics

::: warning
The type scanning mechanics are somewhat brittle when there are dependency issues in your application, and this command can help you spot where problems may be occurring.
:::

The [type scanning](/guide/ioc/diagnostics/type-scanning) can be accessed at the command line with this command:

```bash
dotnet run -- lamar-scanning
```

## Programmatic Diagnostics

The older, programmatic usages of Lamar diagnostics are described in other sections.
