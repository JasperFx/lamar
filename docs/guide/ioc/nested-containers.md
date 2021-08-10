# Nested Containers (Per Request/Transaction)

::: tip INFO
If you're coming from StructureMap, do note that Lamar does not yet support the concept of "child" containers
:::

_Nested Container's_ are a powerful feature in Lamar for service resolution and clean object disposal in the
context of short lived operations like HTTP requests or handling a message within some kind of service bus. A _nested container_
is specific to the scope of that operation and is should not live on outside of that scope.

Here's a sample of a nested container in action:

<!-- snippet: sample_using-nested-container -->
<a id='snippet-sample_using-nested-container'></a>
```cs
[Fact]
public void using_nested_containers()
{
    var container = new Container(x =>
    {
        x.AddSingleton<IWidget, AWidget>();
        x.AddScoped<IService, WhateverService>();
        x.AddTransient<IClock, Clock>();
    });

    var rootWidget = container.GetInstance<IWidget>();
    var rootService = container.GetInstance<IService>();

    var nested = container.GetNestedContainer();
    
    // Singleton scoped objects are the same
    nested.GetInstance<IWidget>()
        .ShouldBeSameAs(rootWidget);
    
    // Scoped objects are specific to the container
    var nestedService = nested.GetInstance<IService>();
    nestedService
        .ShouldNotBeSameAs(rootService);
    
    nested.GetInstance<IService>()
        .ShouldBeSameAs(nestedService);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Samples/NestedContainer.cs#L11-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-nested-container' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You probably won't directly interact with nested containers, but do note that they are used behind the scenes at runtime of basically every
popular application framework in .Net these days (ASP.Net Core, NServiceBus, MassTransit, NancyFx, you name it).
