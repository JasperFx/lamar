# Generic Types

Lamar comes with some power abilities to exploit [open generic types](https://msdn.microsoft.com/en-us/library/ms172334(v=vs.110).aspx) in .Net for extensibility
and flexible handling within your system.

## Example 1: Visualizing an Activity Log

I worked years ago on a system that could be used to record and resolve customer support problems. Since it was very workflow heavy in its logic,
we tracked user and system activity as an _event stream_ of small objects that reflected all the different actions or state changes
that could happen to an issue. To render and visualize the activity log to HTML, we used many of the open generic type capabilities shown in
this topic to find and apply the correct HTML rendering strategy for each type of log object in an activity stream.

Given a log object, we wanted to look up the right visualizer strategy to render that type of log object to html on the server side.

To start, we had an interface like this one that we were going to use to get the HTML for each log object:

<!-- snippet: sample_ILogVisualizer -->
<a id='snippet-sample_ilogvisualizer'></a>
```cs
public interface ILogVisualizer
{
    // If we already know what the type of log we have
    string ToHtml<TLog>(TLog log);

    // If we only know that we have a log object
    string ToHtml(object log);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L154-L165' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ilogvisualizer' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_ilogvisualizer-1'></a>
```cs
public interface ILogVisualizer
{
    // If we already know what the type of log we have
    string ToHtml<TLog>(TLog log);

    // If we only know that we have a log object
    string ToHtml(object log);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/Visualization/VisualizationClasses.cs#L36-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ilogvisualizer-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

So for an example, if we already knew that we had an `IssueCreated` object, we should be able to use Lamar like this:

<!-- snippet: sample_using-visualizer-knowning-the-type -->
<a id='snippet-sample_using-visualizer-knowning-the-type'></a>
```cs
// Just setting up a Container and ILogVisualizer
var container = Container.For<VisualizationRegistry>();
var visualizer = container.GetInstance<ILogVisualizer>();

// If I have an IssueCreated lob object...
var created = new IssueCreated();

// I can get the html representation:
var html = visualizer.ToHtml(created);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L30-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-visualizer-knowning-the-type' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-visualizer-knowning-the-type-1'></a>
```cs
// Just setting up a Container and ILogVisualizer
var container = Container.For<VisualizationRegistry>();
var visualizer = container.GetInstance<ILogVisualizer>();

var items = logs.Select(visualizer.ToHtml);
var html = string.Join("<hr />", items);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L60-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-visualizer-knowning-the-type-1' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-visualizer-knowning-the-type-2'></a>
```cs
// Just setting up a Container and ILogVisualizer
var container = Container.For<VisualizationRegistry>();
var visualizer = container.GetInstance<ILogVisualizer>();

// If I have an IssueCreated lob object...
var created = new IssueCreated();

// I can get the html representation:
var html = visualizer.ToHtml(created);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/generic_types.cs#L36-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-visualizer-knowning-the-type-2' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-visualizer-knowning-the-type-3'></a>
```cs
// Just setting up a Container and ILogVisualizer
var container = Container.For<VisualizationRegistry>();
var visualizer = container.GetInstance<ILogVisualizer>();

var items = logs.Select(visualizer.ToHtml);
var html = string.Join("<hr />", items);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/generic_types.cs#L62-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-visualizer-knowning-the-type-3' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If we had an array of log objects, but we do not already know the specific types, we can still use the more generic `ToHtml(object)` method like this:

<!-- snippet: sample_using-visualizer-not-knowing-the-type -->
<a id='snippet-sample_using-visualizer-not-knowing-the-type'></a>
```cs
var logs = new object[]
{
    new IssueCreated(),
    new TaskAssigned(),
    new Comment(),
    new IssueResolved()
};
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L48-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-visualizer-not-knowing-the-type' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-visualizer-not-knowing-the-type-1'></a>
```cs
var logs = new object[]
{
    new IssueCreated(),
    new TaskAssigned(),
    new Comment(),
    new IssueResolved()
};
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/generic_types.cs#L52-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-visualizer-not-knowing-the-type-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The next step is to create a way to identify the visualization strategy for a single type of log object. We certainly could have done this
with a giant switch statement, but we wanted some extensibility for new types of activity log objects and even customer specific log types
that would never, ever be in the main codebase. We settled on an interface like the one shown below that would be responsible for
rendering a particular type of log object ("T" in the type):

<!-- snippet: sample_IVisualizer_T -->
<a id='snippet-sample_ivisualizer_t'></a>
```cs
public interface IVisualizer<TLog>
{
    string ToHtml(TLog log);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L145-L152' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ivisualizer_t' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Inside of the concrete implementation of `ILogVisualizer` we need to be able to pull out and use the correct `IVisualizer<T>` strategy for a log type. We of course
used a Lamar `Container` to do the resolution and lookup, so now we also need to be able to register all the log visualization strategies in some easy way.
On top of that, many of the log types were simple and could just as easily be rendered with a simple html strategy like this class:

<!-- snippet: sample_DefaultVisualizer -->
<a id='snippet-sample_defaultvisualizer'></a>
```cs
public class DefaultVisualizer<TLog> : IVisualizer<TLog>
{
    public string ToHtml(TLog log)
    {
        return string.Format("<div>{0}</div>", log);
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L167-L177' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_defaultvisualizer' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_defaultvisualizer-1'></a>
```cs
public class DefaultVisualizer<TLog> : IVisualizer<TLog>
{
    public string ToHtml(TLog log)
    {
        return string.Format("<div>{0}</div>", log);
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/Visualization/VisualizationClasses.cs#L47-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_defaultvisualizer-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Inside of our Lamar usage, if we don't have a specific visualizer for a given log type, we'd just like to fallback to the default visualizer and proceed.

Alright, now that we have a real world problem, let's proceed to the mechanics of the solution.

## Registering Open Generic Types

Let's say to begin with all we want to do is to always use the `DefaultVisualizer` for each log type. We can do that with code like this below:

<!-- snippet: sample_register_open_generic_type -->
<a id='snippet-sample_register_open_generic_type'></a>
```cs
[Fact]
public void register_open_generic_type()
{
    var container = new Container(_ => { _.For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>)); });

    container.GetInstance<IVisualizer<IssueCreated>>()
        .ShouldBeOfType<DefaultVisualizer<IssueCreated>>();

    container.GetInstance<IVisualizer<IssueResolved>>()
        .ShouldBeOfType<DefaultVisualizer<IssueResolved>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L9-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_register_open_generic_type' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_register_open_generic_type-1'></a>
```cs
[Fact]
public void register_open_generic_type()
{
    var container = new Container(_ =>
    {
        _.For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));
    });

    Debug.WriteLine(container.WhatDoIHave(@namespace: "StructureMap.Testing.Acceptance.Visualization"));

    container.GetInstance<IVisualizer<IssueCreated>>()
        .ShouldBeOfType<DefaultVisualizer<IssueCreated>>();

    Debug.WriteLine(container.WhatDoIHave(@namespace: "StructureMap.Testing.Acceptance.Visualization"));

    container.GetInstance<IVisualizer<IssueResolved>>()
        .ShouldBeOfType<DefaultVisualizer<IssueResolved>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/generic_types.cs#L11-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_register_open_generic_type-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

With the configuration above, there are no specific registrations for `IVisualizer<IssueCreated>`. At the first request for that
interface, Lamar will run through its ["missing service policies"](/guide/ioc/registration/policies), one of which is
to try to find registrations for an open generic type that could be closed to make a valid registration for the requested type. In the case above,
Lamar sees that it has registrations for the open generic type `IVisualizer<T>` that could be used to create registrations for the
closed type `IVisualizer<IssueCreated>`.

Using the [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) diagnostics, the original state of the container for the visualization namespace is:

```bash
===========================================================================================================================
PluginType            Namespace                                  Lifecycle     Description                 Name
---------------------------------------------------------------------------------------------------------------------------
IVisualizer<TLog>     Lamar.Testing.Acceptance.Visualization     Transient     DefaultVisualizer<TLog>     (Default)
===========================================================================================================================
```

After making a request for `IVisualizer<IssueCreated>`, the new state is:

```bash
====================================================================================================================================================================================
PluginType                    Namespace                                  Lifecycle     Description                                                                  Name
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
IVisualizer<IssueCreated>     Lamar.Testing.Acceptance.Visualization     Transient     DefaultVisualizer<IssueCreated> ('548b4256-a7aa-46a3-8072-bd8ef0c5c430')     (Default)
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
IVisualizer<TLog>             Lamar.Testing.Acceptance.Visualization     Transient     DefaultVisualizer<TLog>                                                      (Default)
====================================================================================================================================================================================
```

## Generic Registrations and Default Fallbacks

A powerful feature of generic type support in Lamar is the ability to register specific handlers for some types, but allow
users to register a "fallback" registration otherwise. In the case of the visualization, some types of log objects may justify some
special HTML rendering while others can happily be rendered with the default visualization strategy. This behavior is demonstrated by
the following code sample:

<!-- snippet: sample_generic-defaults-with-fallback -->
<a id='snippet-sample_generic-defaults-with-fallback'></a>
```cs
[Fact]
public void generic_defaults()
{
    var container = new Container(_ =>
    {
        // The default visualizer just like we did above
        _.For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));

        // Register a specific visualizer for IssueCreated
        _.For<IVisualizer<IssueCreated>>().Use<IssueCreatedVisualizer>();
    });

    // We have a specific visualizer for IssueCreated
    container.GetInstance<IVisualizer<IssueCreated>>()
        .ShouldBeOfType<IssueCreatedVisualizer>();

    // We do not have any special visualizer for TaskAssigned,
    // so fall back to the DefaultVisualizer<T>
    container.GetInstance<IVisualizer<TaskAssigned>>()
        .ShouldBeOfType<DefaultVisualizer<TaskAssigned>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L72-L96' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_generic-defaults-with-fallback' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_generic-defaults-with-fallback-1'></a>
```cs
[Fact]
public void generic_defaults()
{
    var container = new Container(_ =>
    {
        // The default visualizer just like we did above
        _.For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));

        // Register a specific visualizer for IssueCreated
        _.For<IVisualizer<IssueCreated>>().Use<IssueCreatedVisualizer>();
    });

    // We have a specific visualizer for IssueCreated
    container.GetInstance<IVisualizer<IssueCreated>>()
        .ShouldBeOfType<IssueCreatedVisualizer>();

    // We do not have any special visualizer for TaskAssigned,
    // so fall back to the DefaultVisualizer<T>
    container.GetInstance<IVisualizer<TaskAssigned>>()
        .ShouldBeOfType<DefaultVisualizer<TaskAssigned>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/generic_types.cs#L72-L95' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_generic-defaults-with-fallback-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Connecting Generic Implementations with Type Scanning

::: tip INFO
It's generally harmful in software projects to have a single code file that has to be frequently edited to for unrelated changes,
and Lamar `Registry` classes that explicitly configure services can easily fall into that category. Using type scanning registration can help
teams avoid that problem altogether by eliminating the need to make any explicit registrations as new providers are added to the codebase.
:::

For this example, I have two special visualizers for the `IssueCreated` and `IssueResolved` log types:

<!-- snippet: sample_specific-visualizers -->
<a id='snippet-sample_specific-visualizers'></a>
```cs
public class IssueCreatedVisualizer : IVisualizer<IssueCreated>
{
    public string ToHtml(IssueCreated log)
    {
        return "special html for an issue being created";
    }
}

public class IssueResolvedVisualizer : IVisualizer<IssueResolved>
{
    public string ToHtml(IssueResolved log)
    {
        return "special html for issue resolved";
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L203-L221' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_specific-visualizers' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_specific-visualizers-1'></a>
```cs
public class IssueCreatedVisualizer : IVisualizer<IssueCreated>
{
    public string ToHtml(IssueCreated log)
    {
        return "special html for an issue being created";
    }
}

public class IssueResolvedVisualizer : IVisualizer<IssueResolved>
{
    public string ToHtml(IssueResolved log)
    {
        return "special html for issue resolved";
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/Visualization/VisualizationClasses.cs#L68-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_specific-visualizers-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In the real project that inspired this example, we had many, many more types of log visualizer strategies and it
could have easily been very tedious to manually register all the different little `IVisualizer<T>` strategy types in a `Registry` class by hand.
Fortunately, part of Lamar's [type scanning](/guide/ioc/registration/auto-registration-and-conventions) support is the `ConnectImplementationsToTypesClosing()`
auto-registration mechanism via generic templates for exactly this kind of scenario.

In the sample below, I've set up a type scanning operation that will register any concrete type in the Assembly that contains the `VisualizationRegistry`
that closes `IVisualizer<T>` against the proper interface:

<!-- snippet: sample_VisualizationRegistry -->
<a id='snippet-sample_visualizationregistry'></a>
```cs
public class VisualizationRegistry : ServiceRegistry
{
    public VisualizationRegistry()
    {
        // The main ILogVisualizer service
        For<ILogVisualizer>().Use<LogVisualizer>();

        // A default, fallback visualizer
        For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));

        // Auto-register all concrete types that "close"
        // IVisualizer<TLog>
        Scan(x =>
        {
            x.TheCallingAssembly();
            x.ConnectImplementationsToTypesClosing(typeof(IVisualizer<>));
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L121-L143' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_visualizationregistry' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_visualizationregistry-1'></a>
```cs
public class VisualizationRegistry : Registry
{
    public VisualizationRegistry()
    {
        // The main ILogVisualizer service
        For<ILogVisualizer>().Use<LogVisualizer>();

        // A default, fallback visualizer
        For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));

        // Auto-register all concrete types that "close"
        // IVisualizer<TLog>
        Scan(x =>
        {
            x.TheCallingAssembly();
            x.ConnectImplementationsToTypesClosing(typeof(IVisualizer<>));
        });

    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/Visualization/VisualizationClasses.cs#L6-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_visualizationregistry-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If we create a `Container` based on the configuration above, we can see that the type scanning operation picks up the specific visualizers for
`IssueCreated` and `IssueResolved` as shown in the diagnostic view below:

```bash
==================================================================================================================================================================================
PluginType                     Namespace                                  Lifecycle     Description                                                               Name
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
ILogVisualizer                 Lamar.Testing.Acceptance.Visualization     Transient     Lamar.Testing.Acceptance.Visualization.LogVisualizer                      (Default)
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
IVisualizer<IssueResolved>     Lamar.Testing.Acceptance.Visualization     Transient     Lamar.Testing.Acceptance.Visualization.IssueResolvedVisualizer            (Default)
                                                                          Transient     DefaultVisualizer<IssueResolved>
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
IVisualizer<IssueCreated>      Lamar.Testing.Acceptance.Visualization     Transient     Lamar.Testing.Acceptance.Visualization.IssueCreatedVisualizer             (Default)
                                                                          Transient     DefaultVisualizer<IssueCreated>
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
IVisualizer<TLog>              Lamar.Testing.Acceptance.Visualization     Transient     DefaultVisualizer<TLog>                                                   (Default)
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
IVisualizer<TLog>              Lamar.Testing.Acceptance.Visualization     Transient     DefaultVisualizer<TLog>                                                   (Default)
==================================================================================================================================================================================

```

The following sample shows the `VisualizationRegistry` in action to combine the type scanning registration plus the default fallback behavior for
log types that do not have any special visualization logic:

<!-- snippet: sample_visualization-registry-in-action -->
<a id='snippet-sample_visualization-registry-in-action'></a>
```cs
[Fact]
public void visualization_registry()
{
    var container = Container.For<VisualizationRegistry>();

    container.GetInstance<IVisualizer<IssueCreated>>()
        .ShouldBeOfType<IssueCreatedVisualizer>();

    container.GetInstance<IVisualizer<IssueResolved>>()
        .ShouldBeOfType<IssueResolvedVisualizer>();

    // We have no special registration for TaskAssigned,
    // so fallback to the default visualizer
    container.GetInstance<IVisualizer<TaskAssigned>>()
        .ShouldBeOfType<DefaultVisualizer<TaskAssigned>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/generic_types.cs#L98-L118' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_visualization-registry-in-action' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_visualization-registry-in-action-1'></a>
```cs
[Fact]
public void visualization_registry()
{
    var container = Container.For<VisualizationRegistry>();

    Debug.WriteLine(container.WhatDoIHave(@namespace: "StructureMap.Testing.Acceptance.Visualization"));

    container.GetInstance<IVisualizer<IssueCreated>>()
        .ShouldBeOfType<IssueCreatedVisualizer>();

    container.GetInstance<IVisualizer<IssueResolved>>()
        .ShouldBeOfType<IssueResolvedVisualizer>();

    // We have no special registration for TaskAssigned,
    // so fallback to the default visualizer
    container.GetInstance<IVisualizer<TaskAssigned>>()
        .ShouldBeOfType<DefaultVisualizer<TaskAssigned>>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/generic_types.cs#L97-L117' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_visualization-registry-in-action-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
