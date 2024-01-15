# Setter Injection

::: tip INFO
In all cases, *Setter Injection* is an opt-in feature in Lamar that has to be explicitly enabled on a case by case basis.
:::

::: warning
All the *Setter Injection* rules and attributes are ignored at runtime if Lamar does not know how to resolve the property type. If setter injection is not working for you, try to look at the [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) output and [type scanning](/guide/ioc/diagnostics/type-scanning).
:::

Lamar can inject dependencies into public setter properties as part of its construction process using the _Setter Injection_ form of Dependency Injection. However, the Lamar team strongly recommends using constructor injection wherever possible instead of setter injection. That being said,
there are few cases where setter injection is probably easier (inheritance hierarchies), not to mention legacy or third party tools that
simply cannot support constructor injection *cough* ASP.Net *cough*.

See this discussion from Martin Fowler on [Constructor vs Setter Injection](http://martinfowler.com/articles/injection.html#ConstructorVersusSetterInjection).

**If you are having any trouble with setter injection in your Lamar usage, make sure you're familiar with using [build plans](/guide/ioc/diagnostics/build-plans)
to help in troubleshooting**

## Explicit Setter Injection with [SetterProperty] Attributes

The simplest conceptual way to force Lamar into making public setters mandatory service dependencies by decorating setter properties with the `[SetterProperty]` attribute like this example:

<!-- snippet: sample_setter-injection-with-SetterProperty -->
<a id='snippet-sample_setter-injection-with-setterproperty'></a>
```cs
public class Repository
{
    // Adding the SetterProperty to a setter directs
    // Lamar to use this property when
    // constructing a Repository instance
    [SetterProperty] public IDataProvider Provider { get; set; }

    [SetterProperty] public bool ShouldCache { get; set; }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/SetterExamples.cs#L17-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_setter-injection-with-setterproperty' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Without the `[SetterProperty]` attributes decorating the setters, Lamar would ignore the `Provider` and `ShouldCache` properties when it builds up a `Repository` object. With the attributes, Lamar will try to build and attach values for the two properties as part of object construction.

If you were to look at Lamar's "build plan" for the `Repository` class, you would see the actual C# code that Lamar compiles to build the concrete objects:

```csharp
public class Lamar_Testing_Examples_Repository_repository : Lamar.IoC.Resolvers.TransientResolver<Lamar.Testing.Examples.Repository>
{

    public bool func_repository_bool {get; set;}


    public override Lamar.Testing.Examples.Repository Build(Lamar.IoC.Scope scope)
    {
        var dataProvider = new Lamar.Testing.Examples.DataProvider();
        return new Lamar.Testing.Examples.Repository(){Provider = dataProvider, ShouldCache = func_repository_bool};
    }

}
```

Alas, like almost every code generation tool in the history of computer science, the resulting code isn't terribly pretty. You may find it easier to read and parse by copying the code into a real class file and letting your tool of choice (ReSharper or Rider for me) reformat the code and clean up.

If you intensely dislike runaway attribute usage, that's okay because there are other ways to enable setter injection in Lamar.

## Inline Setter Configuration

Any setter property not configured with `[SetterProperty]` or the setter policies in the next section can still be filled by Lamar if an inline dependency is configured matching that setter property as shown in the example below:

<!-- snippet: sample_inline-dependencies-setters -->
<a id='snippet-sample_inline-dependencies-setters'></a>
```cs
public class RuleWithSetters : IEventRule
{
    public ICondition Condition { get; set; }
    public IAction Action { get; set; }

    public void ProcessEvent(SomeEvent @event)
    {
        if (Condition.Matches(@event))
        {
            Action.PerformWork(@event);
        }
    }
}

public class RuleWithSettersRegistry : ServiceRegistry
{
    public RuleWithSettersRegistry()
    {
        For<IEventRule>().Use<RuleWithSetters>()
            .Setter<ICondition>().Is<Condition1>()

            // or if you need to specify the property name
            .Setter<IAction>("Action").Is<Action2>();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L238-L266' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-setters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

See also: [Inline Dependencies](/guide/ioc/registration/inline-dependencies)

## Setter Injection Policies

Lastly, you can give Lamar some criteria for determining which setters should be mandatory dependencies with the `Registry.Policies.SetAllProperties()` method during Container setup as shown in this example below:

<!-- snippet: sample_using-setter-policy -->
<a id='snippet-sample_using-setter-policy'></a>
```cs
public class ClassWithNamedProperties
{
    public int Age { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public IGateway Gateway { get; set; }
    public IService Service { get; set; }
}

[Fact]
public void specify_setter_policy_and_construct_an_object()
{
    var theService = new ColorService("red");

    var container = new Container(x =>
    {
        x.For<IService>().Use(theService);
        x.For<IGateway>().Use<DefaultGateway>();

        x.ForConcreteType<ClassWithNamedProperties>().Configure.Setter<int>().Is(5);

        x.Policies.SetAllProperties(
            policy => policy.WithAnyTypeFromNamespace("StructureMap.Testing.Widget3"));
    });

    var description = container.Model.For<ClassWithNamedProperties>().Default.DescribeBuildPlan();
    Debug.WriteLine(description);

    var target = container.GetInstance<ClassWithNamedProperties>();
    target.Service.ShouldBeSameAs(theService);
    target.Gateway.ShouldBeOfType<DefaultGateway>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/setter_injection.cs#L318-L353' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-setter-policy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

All calls to `Registry.Policies.SetAllProperties()` are additive, meaning you can use as many criteria as possible for setter injection.
