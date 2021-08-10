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

<[sample:ILogVisualizer]>

So for an example, if we already knew that we had an `IssueCreated` object, we should be able to use Lamar like this:

<[sample:using-visualizer-knowning-the-type]>

If we had an array of log objects, but we do not already know the specific types, we can still use the more generic `ToHtml(object)` method like this:

<[sample:using-visualizer-not-knowing-the-type]>

The next step is to create a way to identify the visualization strategy for a single type of log object. We certainly could have done this
with a giant switch statement, but we wanted some extensibility for new types of activity log objects and even customer specific log types
that would never, ever be in the main codebase. We settled on an interface like the one shown below that would be responsible for
rendering a particular type of log object ("T" in the type):

<[sample:IVisualizer\<T\>]>

Inside of the concrete implementation of `ILogVisualizer` we need to be able to pull out and use the correct `IVisualizer<T>` strategy for a log type. We of course
used a Lamar `Container` to do the resolution and lookup, so now we also need to be able to register all the log visualization strategies in some easy way.
On top of that, many of the log types were simple and could just as easily be rendered with a simple html strategy like this class:

<[sample:DefaultVisualizer]>

Inside of our Lamar usage, if we don't have a specific visualizer for a given log type, we'd just like to fallback to the default visualizer and proceed.

Alright, now that we have a real world problem, let's proceed to the mechanics of the solution.

## Registering Open Generic Types

Let's say to begin with all we want to do is to always use the `DefaultVisualizer` for each log type. We can do that with code like this below:

<[sample:register_open_generic_type]>

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

<[sample:generic-defaults-with-fallback]>

## Connecting Generic Implementations with Type Scanning

::: tip INFO
It's generally harmful in software projects to have a single code file that has to be frequently edited to for unrelated changes,
and Lamar `Registry` classes that explicitly configure services can easily fall into that category. Using type scanning registration can help
teams avoid that problem altogether by eliminating the need to make any explicit registrations as new providers are added to the codebase.
:::

For this example, I have two special visualizers for the `IssueCreated` and `IssueResolved` log types:

<[sample:specific-visualizers]>

In the real project that inspired this example, we had many, many more types of log visualizer strategies and it
could have easily been very tedious to manually register all the different little `IVisualizer<T>` strategy types in a `Registry` class by hand.
Fortunately, part of Lamar's [type scanning](/guide/ioc/registration/auto-registration-and-conventions) support is the `ConnectImplementationsToTypesClosing()`
auto-registration mechanism via generic templates for exactly this kind of scenario.

In the sample below, I've set up a type scanning operation that will register any concrete type in the Assembly that contains the `VisualizationRegistry`
that closes `IVisualizer<T>` against the proper interface:

<[sample:VisualizationRegistry]>

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

<[sample:visualization-registry-in-action]>
