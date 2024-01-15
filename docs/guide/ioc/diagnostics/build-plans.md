# Build Plans

As part of the diagnostics, you can preview the generated code that Lamar has used to resolve a service to completely understand how that object and all of its
dependencies are built out.

## HowDoIBuild()

New for Lamar 3.1.0 is a convenience method similar to `WhatDoIHave()` that prints out the build plans:

<!-- snippet: sample_using-HowDoIBuild -->
<a id='snippet-sample_using-howdoibuild'></a>
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

Console.WriteLine(container.HowDoIBuild());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/HowDoIBuild_smoke_tests.cs#L30-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-howdoibuild' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

  This method also provides the same kind of filtering as the [WhatDoIHave](/guide/ioc/diagnostics/what-do-i-have) operation.

## Querying for Specific Build Plans

Let's say you have a container configured like this:

<!-- snippet: sample_container-for-build-plan -->
<a id='snippet-sample_container-for-build-plan'></a>
```cs
container = new Container(x =>
{
    x.For(typeof(IService<>)).Add(typeof(Service<>));
    x.For(typeof(IService<>)).Add(typeof(Service2<>));

    x.For<IWidget>().Use<AWidget>().Singleton();

    x.AddTransient<Rule, DefaultRule>();
    x.AddTransient<Rule, ARule>();
    x.AddSingleton<Rule>(new ColorRule("red"));

    x.AddScoped<IThing, Thing>();

    x.For<IEngine>().Use<PushrodEngine>();

    x.For<Startable1>().Use<Startable1>().Singleton();
    x.For<Startable2>().Use<Startable2>();
    x.For<Startable3>().Use<Startable3>();
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/container_model_usage.cs#L23-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_container-for-build-plan' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And you have a concrete type like this one:

<!-- snippet: sample_UsesStuff -->
<a id='snippet-sample_usesstuff'></a>
```cs
public class UsesStuff
{
    public UsesStuff(IWidget widget, IThing thing, IEngine engine)
    {
        Widget = widget;
        Thing = thing;
        Engine = engine;
    }

    public IWidget Widget { get; }
    public IThing Thing { get; }
    public IEngine Engine { get; }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/container_model_usage.cs#L231-L247' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_usesstuff' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To see what the generated code is to resolve that `UsesStuff` type, we can use the [container diagnostic model](/guide/ioc/diagnostics/using-the-container-model) to access that code for us with this syntax:

<!-- snippet: sample_getting-build-plan -->
<a id='snippet-sample_getting-build-plan'></a>
```cs
var plan = container.Model.For<UsesStuff>().Default.DescribeBuildPlan();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/container_model_usage.cs#L52-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getting-build-plan' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which outputs this lovely looking code below:

```csharp
public class Lamar_Testing_IoC_Acceptance_container_model_usage_UsesStuff_usesStuff : Lamar.IoC.Resolvers.TransientResolver<Lamar.Testing.IoC.Acceptance.container_model_usage.UsesStuff>
{
    private readonly IWidget _widget;

    public Lamar_Testing_IoC_Acceptance_container_model_usage_UsesStuff_usesStuff(IWidget widget)
    {
        _widget = widget;
    }



    public override Lamar.Testing.IoC.Acceptance.container_model_usage.UsesStuff Build(Lamar.IoC.Scope scope)
    {
        var pushrodEngine = new Lamar.Testing.IoC.Acceptance.container_model_usage.PushrodEngine();
        var thing = scope.GetInstance<Lamar.Testing.IoC.Acceptance.IThing>("thing");
        return new Lamar.Testing.IoC.Acceptance.container_model_usage.UsesStuff(_widget, thing, pushrodEngine);
    }

}
```

Some notes on this:

* You'll see that dependencies marked as `Transient` are just built inline (`PushrodEngine`)
* Dependencies marked as `Singleton` end up being constructor dependencies to the resolver class and effectively inlined. That's a pretty significant performance that most modern IoC tools make in some form or fashion
* `Scoped` dependencies -- for the moment -- are accessed by using service location to get the scoping right and honestly just to simplify the code generation process
