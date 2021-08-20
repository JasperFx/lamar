# Working with Enumerable Types

::: warning
Be aware that the Lamar registrations and execution plans for enumerable types that are not explicitly registered are created on the first usage **and will not appear in the WhatDoIHave() output until they are used as either a dependency for another service or directly through a service location call for the first time**. This is normal, as expected behavior.
:::

While you can certainly use *any* `IEnumerable` type as a service type with your own explicit configuration,
Lamar has *some* built in support for these specific enumerable types:

1. `IEnumerable<T>`
1. `IList<T>`
1. `List<T>`
1. `ICollection<T>`
1. `T[]`

Specifically, if you request one of these types either directly with `GetInstance<IList<IWidget>>()` or as a declared
dependency in a constructor or setter (`new WidgetUser(IList<IWidgets> widgets)` for example) and you have no
specific registration for the enumerable types, Lamar has a built in policy to return all the registered instances
of `IWidget` **in the exact order that the registrations were made to Lamar**.

Note, if there are not any registrations for whatever `T` is, you'll get an empty enumeration.

Here's an acceptance test from the Lamar codebase that demonstrates this:

<!-- snippet: sample_EnumerableFamilyPolicy_in_action -->
<a id='snippet-sample_enumerablefamilypolicy_in_action'></a>
```cs
[Fact]
public void collection_types_are_all_possible_by_default()
{
    // NOTE that we do NOT make any explicit registration of
    // IList<IWidget>, IEnumerable<IWidget>, ICollection<IWidget>, or IWidget[]
    var container = new Container(_ =>
    {
        _.For<IWidget>().Add<AWidget>();
        _.For<IWidget>().Add<BWidget>();
        _.For<IWidget>().Add<CWidget>();
    });

    // IList<T>
    container.GetInstance<IList<IWidget>>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    // ICollection<T>
    container.GetInstance<ICollection<IWidget>>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    // Array of T
    container.GetInstance<IWidget[]>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/enumerable_instances.cs#L10-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enumerablefamilypolicy_in_action' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And another showing how you can override this behavior with explicit configuration:

<!-- snippet: sample_explicit-enumeration-behavior -->
<a id='snippet-sample_explicit-enumeration-behavior'></a>
```cs
[Fact]
public void override_enumeration_behavior()
{
    var container = new Container(_ =>
    {
        _.For<IWidget>().Add<AWidget>();
        _.For<IWidget>().Add<BWidget>();
        _.For<IWidget>().Add<CWidget>();

        // Explicit registration should have precedence over the default
        // behavior
        _.For<IWidget[]>().Use(new IWidget[] { new DefaultWidget() });
    });

    container.GetInstance<IWidget[]>()
        .Single().ShouldBeOfType<DefaultWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/enumerable_instances.cs#L41-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_explicit-enumeration-behavior' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Sample Usage: Validation Rules

One of the ways that I have used the built in `IEnumerable` handling is for extensible validation rules. Say that we are
building a system to process `IWidget` objects. As part of processing a widget, we first need to validate that widget with a
series of rules that we might model with the `IWidgetValidator` interface shown below and used within the main
`WidgetProcessor` class:

<!-- snippet: sample_IWidgetValidator-enumerable -->
<a id='snippet-sample_iwidgetvalidator-enumerable'></a>
```cs
public interface IWidgetValidator
{
    IEnumerable<string> Validate(IWidget widget);
}

public class WidgetProcessor
{
    private readonly IEnumerable<IWidgetValidator> _validators;

    public WidgetProcessor(IEnumerable<IWidgetValidator> validators)
    {
        _validators = validators;
    }

    public void Process(IWidget widget)
    {
        var validationMessages = _validators.SelectMany(x => x.Validate(widget))
            .ToArray();

        if (validationMessages.Any())
        {
            // don't process the widget
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/enumerable_instances.cs#L62-L89' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iwidgetvalidator-enumerable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

We *could* simply configure all of the `IWidgetValidator` rules in one place with an explicit registration of `IEnumerable<IWidgetValidator>`,
but what if we need to have an extensibility to add more validation rules later? What if we want to add these additional rules in addon packages? Or we
just don't want to continuously break into the centralized `Registry` class every single time we add a new validation rule?

By relying on Lamar's `IEnumerable` behavior, we're able to split our `IWidgetValidatior` registration across multiple `Registry` classes and that's not infrequently useful to do.
