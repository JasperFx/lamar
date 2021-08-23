# Decorators, Interceptors, and Activators

::: tip
If you need some kind of interception policy that doesn't fit into the admittedly simpler
examples shown here, just ask for help in the [Lamar Gitter room](https://gitter.im/JasperFx/Lamar).
:::

Lamar v5 finally added the full complement of interceptors that users have grown to expect from .Net IoC containers. To differentiate between the three terms, Lamar
now supports:

1. *Decorators* - the classic [Gang of Four Decorator](https://en.wikipedia.org/wiki/Decorator_pattern) pattern where at runtime Lamar will "wrap" a 
   concrete decorator object around the registered service
1. *Interceptors* - in a user supplied Lambda, get a chance to work with the newly built object or even return a different object with a possibly [dynamic proxy](https://stackoverflow.com/questions/933993/what-are-dynamic-proxy-classes-and-why-would-i-use-one)
1. *Activators* - a user supplied Lambda that is called right after an object is created by Lamar, but before it is returned to the caller 

## Decorators

::: tip
When you use a decorator in Lamar, it effectively replaces the original registration with a new registration for the concrete decorator
type, then moves the original registration to be an inline dependency of the decorated type. Be aware that the `WhatDoIHave()` display
will show the concrete decorator type instead of the original registration because of this.
:::

If you look in the Lamar codebase, you'll find dozens of tests that use a fake type named `IWidget` that *does something*:

<!-- snippet: sample_IWidget -->
<a id='snippet-sample_iwidget'></a>
```cs
public interface IWidget
{
    void DoSomething();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing.Widget/IWidget.cs#L7-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iwidget' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Let's say that we have service registrations in our system for that `IWidget` interface, but we want each of them to be decorated by another form of `IWidget` like this to perform some kind of
cross cutting concern before and/or after the call to the specific widget:

<!-- snippet: sample_WidgetHolder-Decorator -->
<a id='snippet-sample_widgetholder-decorator'></a>
```cs
public class WidgetDecorator : IWidget
{
    public WidgetDecorator(IThing thing, IWidget inner)
    {
        Inner = inner;
    }

    public IWidget Inner { get; }
    
    public void DoSomething()
    {
        // do something before 
        Inner.DoSomething();
        // do something after
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/decorators.cs#L235-L252' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_widgetholder-decorator' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

We can configure Lamar to add a decorator around all other `IWidget` registrations with this syntax:

<!-- snippet: sample_decorator-sample -->
<a id='snippet-sample_decorator-sample'></a>
```cs
var container = new Container(_ =>
{
    // This usage adds the WidgetHolder as a decorator
    // on all IWidget registrations
    _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
    
    // The AWidget type will be decorated w/ 
    // WidgetHolder when you resolve it from the container
    _.For<IWidget>().Use<AWidget>();
    
    _.For<IThing>().Use<Thing>();
});

// Just proving that it actually works;)
container.GetInstance<IWidget>()
    .ShouldBeOfType<WidgetDecorator>()
    .Inner.ShouldBeOfType<AWidget>();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/decorators.cs#L18-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_decorator-sample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Activators

::: tip
Activators have to be synchronous at this point, so there might have to be some `GetAwaiter().GetResult()`
action happening to force asynchronous methods to run inline. #sadtrombone.
:::

An *activator* gives you the chance to make some kind of call against an object that has just
been built by Lamar, but before that object is passed to the original service requester.

As an example, let's say that you have some sort of service in your system that is responsible
for polling an external web service for more information. If you're like me, you really don't like
to perform any kind of work besides putting together necessary state in a constructor function,
so you likely have some kind of `Start()` method on the polling service to actually start
things up like this class:

snippet: sample_poller

When we register the class above with Lamar, we can supply a Lambda function to start up
`Poller` like this:

snippet: sample_using_activator_on_one_registration

In the sample above, we registered an activator on one and only one service registration. Lamar
will also let you apply an activator across service registrations that implement or inherit from
a common type. Back to the polling example, what if we introduce a new marker interface
to denote objects that need to be started or activated like this:

snippet: sample_IStartable

Now, let's create a new `StartablePoller`:

snippet: sample_StartablePoller

And going back to the `IPoller` registration, we could now do this:

snippet: sample_activator_by_marker_type

The call to `For<T>().OnCreationForAll(Action<T>)` will apply to any registrations of
type `T` or any registrations where the implementation type can be cast to `T`. So in th case
above, the activator will be applied to the `StartablePoller` type.

As a last example, you also have an overload to use the constructing container in
an activator if you need access to other services. Let's say that for some kind of diagnostic
in our system, we want to track what `IStartable` objects have been created and floating
around in our system at any time with this simple class:

snippet: sample_StartableTracker

Now, we'd like our `IStartable` objects to both `Start()` and be tracked by the class above,
so we'll use an activator like this:

snippet: sample_startable_and_tracker_registration

## Interceptors

::: tip
The main usage for this feature is most likely for dynamic proxies and Aspect Oriented
Programming
:::

*Interceptors* are similar to *activators*, but with two differences:

1. *Interceptors* allow you to return a different object than the one initially created
   by Lamar
1. Because of the potential to return a new object, interceptors are applied at the 
   service type level
   
Here's a sample from the unit tests:

snippet: sample_intercept_a_single_instance

Just like activators, there is the option to only use the original object or the ability
to use the original object and the constructing container.
   
