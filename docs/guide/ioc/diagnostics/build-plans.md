# Build Plans

As part of the diagnostics, you can preview the generated code that Lamar has used to resolve a service to completely understand how that object and all of its
dependencies are built out.

## HowDoIBuild()

New for Lamar 3.1.0 is a convenience method similar to `WhatDoIHave()` that prints out the build plans:

<[sample:using-HowDoIBuild]>

  This method also provides the same kind of filtering as the [WhatDoIHave](/guide/ioc/diagnostics/what-do-i-have) operation.

## Querying for Specific Build Plans

Let's say you have a container configured like this:

<[sample:container-for-build-plan]>

And you have a concrete type like this one:

<[sample:UsesStuff]>

To see what the generated code is to resolve that `UsesStuff` type, we can use the [container diagnostic model](/guide/ioc/diagnostics/using-the-container-model) to access that code for us with this syntax:

<[sample:getting-build-plan]>

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
