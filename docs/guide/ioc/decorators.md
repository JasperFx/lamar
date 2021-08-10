# Decorators

For version 1.0, Lamar's only support for interception is decorators. If you look in the Lamar codebase, you'll find dozens of tests that use a fake type named `IWidget`:

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

Let's say that we have service registrations in our system for that `IWidget` interface, but we want each of them to be decorated by another form of `IWidget` like this:

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
        
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/decorators.cs#L234-L249' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_widgetholder-decorator' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/decorators.cs#L17-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_decorator-sample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
