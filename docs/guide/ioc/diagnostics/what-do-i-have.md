# WhatDoIHave()

The `IContainer.WhatDoIHave()` method can give you a quick textual report of the current configuration of a running `Container`:

<!-- snippet: sample_whatdoihave-simple -->
<a id='snippet-sample_whatdoihave-simple'></a>
```cs
var container = Container.Empty();
var report = container.WhatDoIHave();

Console.WriteLine(report);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDoIHave_smoke_tests.cs#L21-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave-simple' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_whatdoihave-simple-1'></a>
```cs
var container = new Container();
var report = container.WhatDoIHave();

Debug.WriteLine(report);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDoIHave_Smoke_Tester.cs#L14-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave-simple-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Enough talk, say you have a `Container` with this configuration:

<!-- snippet: sample_what_do_i_have_container -->
<a id='snippet-sample_what_do_i_have_container'></a>
```cs
var container = new Container(x =>
{
    x.For<IEngine>().Use<Hemi>().Named("The Hemi");

    x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
    x.For<IEngine>().Add<FourFiftyFour>();
    x.For<IEngine>().Add<StraightSix>().Scoped();

    x.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
    x.For<IEngine>().Add(c => c.GetService<PluginElectric>());

    x.For<IEngine>().Add(new InlineFour());

    x.For<IEngine>().UseIfNone<VTwelve>();
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDoIHave_smoke_tests.cs#L34-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_what_do_i_have_container' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_what_do_i_have_container-1'></a>
```cs
var container = new Container(x =>
{
    x.For<IEngine>().Use<Hemi>().Named("The Hemi");

    x.For<IEngine>().Add<VEight>().Singleton().Named("V8");
    x.For<IEngine>().Add<FourFiftyFour>().AlwaysUnique();
    x.For<IEngine>().Add<StraightSix>().LifecycleIs<ThreadLocalStorageLifecycle>();

    x.For<IEngine>().Add(() => new Rotary()).Named("Rotary");
    x.For<IEngine>().Add(c => c.GetInstance<PluginElectric>());

    x.For<IEngine>().Add(new InlineFour());

    x.For<IEngine>().UseIfNone<VTwelve>();
    x.For<IEngine>().MissingNamedInstanceIs.ConstructedBy(c => new NamedEngine(c.RequestedName));
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDoIHave_Smoke_Tester.cs#L25-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_what_do_i_have_container-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If you were to run the code below against this `Container`:

<!-- snippet: sample_whatdoihave_everything -->
<a id='snippet-sample_whatdoihave_everything'></a>
```cs
Console.WriteLine(container.WhatDoIHave());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDoIHave_smoke_tests.cs#L54-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave_everything' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_whatdoihave_everything-1'></a>
```cs
Debug.WriteLine(container.WhatDoIHave());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDoIHave_Smoke_Tester.cs#L44-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave_everything-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

  you would get the output shown in [this gist](https://gist.github.com/jeremydmiller/7eae90eda21cc47ed24fa30623f9feb2).

If you're curious, all the raw code for this example is in [here](https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDoIHave_smoke_tests.cs).

## Filtering WhatDoIHave()

Filtering the `WhatDoIHave()` results can be done in these ways:

<!-- snippet: sample_whatdoihave-filtering -->
<a id='snippet-sample_whatdoihave-filtering'></a>
```cs
var container = Container.Empty();

// Filter by the Assembly of the Service Type
var byAssembly = container.WhatDoIHave(assembly: typeof(IWidget).Assembly);

// Only report on the specified Service Type
var byServiceType = container.WhatDoIHave(typeof(IWidget));

// Filter to Service Type's in the named namespace
// The 'IsInNamespace' test will include child namespaces
var byNamespace = container.WhatDoIHave(@namespace: "StructureMap.Testing.Widget");

// Filter by a case insensitive string.Contains() match
// against the Service Type name
var byType = container.WhatDoIHave(typeName: "Widget");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDoIHave_smoke_tests.cs#L114-L132' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave-filtering' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_whatdoihave-filtering-1'></a>
```cs
var container = new Container();

// Filter by the Assembly of the Plugin Type
var byAssembly = container.WhatDoIHave(assembly: typeof(IWidget).GetAssembly());

// Only report on the specified Plugin Type
var byPluginType = container.WhatDoIHave(typeof(IWidget));

// Filter to Plugin Type's in the named namespace
// The 'IsInNamespace' test will include child namespaces
var byNamespace = container.WhatDoIHave(@namespace: "StructureMap.Testing.Widget");

// Filter by a case insensitive string.Contains() match
// against the Plugin Type name
var byType = container.WhatDoIHave(typeName: "Widget");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDoIHave_Smoke_Tester.cs#L159-L175' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave-filtering-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## WhatDoIHave() under ASP.Net Core

You can call `WhatDoIHave()` and `WhatDidIScan()` when running in ASP.Net Core like so:

<!-- snippet: sample_whatdoihave-aspnetcore -->
<a id='snippet-sample_whatdoihave-aspnetcore'></a>
```cs
public class StartupWithDiagnostics
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

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if(env.IsDevelopment())
        {
            var container = (IContainer)app.ApplicationServices;
            // or write to your own Logger
            Console.WriteLine(container.WhatDidIScan());
            Console.WriteLine(container.WhatDoIHave());
        }

        app.UseMvc();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.AspNetCoreTests/Samples/StartUp.cs#L63-L97' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdoihave-aspnetcore' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
