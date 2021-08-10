# Inline Dependencies

While you generally allow Lamar to just use _auto-wiring_ to fill the dependencies of a concrete type, there are times when you may want to explicitly configure individual dependencies on a case by case basis. In the case of _primitive_ types like strings or numbers, Lamar **will not** do any auto-wiring, so it's incumbent upon you the user to supply the dependency.

Let's say we have a simple class called `ColorWidget` like the following:

<!-- snippet: sample_inline-dependencies-ColorWidget -->
<a id='snippet-sample_inline-dependencies-colorwidget'></a>
```cs
public class ColorWidget : IWidget
{
    public ColorWidget(string color)
    {
        Color = color;
    }

    public string Color { get; }

    #region ICloneable Members

    public object Clone()
    {
        return MemberwiseClone();
    }
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing.Widget/IWidget.cs#L14-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-colorwidget' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To register the `ColorWidget`, you would supply the value of the `color` parameter to the constructor function like so:

<!-- snippet: sample_inline-dependencies-value -->
<a id='snippet-sample_inline-dependencies-value'></a>
```cs
[Fact]
public void inline_usage_of_primitive_constructor_argument()
{
    var container = new Container(_ =>
    {
        _.For<IWidget>().Use<ColorWidget>()
            .Ctor<string>().Is("Red");
    });

    container.GetInstance<IWidget>()
        .ShouldBeOfType<ColorWidget>()
        .Color.ShouldBe("Red");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L19-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-value' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Event Condition Action Rules

The ability to explicitly define dependencies inline isn't commonly used these days, but was actually one of the very core use cases in the initial versions of Lamar. One of the first usages of Lamar in a production application was in a configurable rules engine using an [Event-Condition-Action](http://en.wikipedia.org/wiki/Event_condition_action) architecture where the conditions and actions were configured in Lamar as inline dependencies of _Rule_ objects. Using Lamar's old Xml configuration, we could define rules for new customers by registering rule objects with the container that reused existing _condition_ and _action_ classes in new configurations.

To make that concrete and establish a sample problem domain, consider these types:

<!-- snippet: sample_inline-dependencies-rule-classes -->
<a id='snippet-sample_inline-dependencies-rule-classes'></a>
```cs
public class SomeEvent
{
}

public interface ICondition
{
    bool Matches(SomeEvent @event);
}

public interface IAction
{
    void PerformWork(SomeEvent @event);
}

public interface IEventRule
{
    void ProcessEvent(SomeEvent @event);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L36-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-rule-classes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, let's move on to seeing how we could use inline dependency configuration to register new rules.

## Constructor Parameters by Type

First off, let's say that we have a `SimpleRule` that takes a single condition and action:

<!-- snippet: sample_inline-dependencies-SimpleRule -->
<a id='snippet-sample_inline-dependencies-simplerule'></a>
```cs
public class SimpleRule : IEventRule
{
    private readonly ICondition _condition;
    private readonly IAction _action;

    public SimpleRule(ICondition condition, IAction action)
    {
        _condition = condition;
        _action = action;
    }

    public void ProcessEvent(SomeEvent @event)
    {
        if (_condition.Matches(@event))
        {
            _action.PerformWork(@event);
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L58-L79' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-simplerule' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, since `SimpleRule` has only a single dependency on both `IAction` and `ICondition`, we can create new rules by registering new Instance's of `SimpleRule` with different combinations of its dependencies:

<!-- snippet: sample_inline-dependencies-simple-ctor-injection -->
<a id='snippet-sample_inline-dependencies-simple-ctor-injection'></a>
```cs
public class InlineCtorArgs : ServiceRegistry
{
    public InlineCtorArgs()
    {
        // Defining args by type
        For<IEventRule>().Use<SimpleRule>()
            .Ctor<ICondition>().Is<Condition1>()
            .Ctor<IAction>().Is<Action1>()
            .Named("One");

        // Pass the explicit values for dependencies
        For<IEventRule>().Use<SimpleRule>()
            .Ctor<ICondition>().Is(new Condition2())
            .Ctor<IAction>().Is(new Action2())
            .Named("Two");

        // Rarely used, but gives you a "do any crazy thing" option
        // Pass in your own Instance object
        For<IEventRule>().Use<SimpleRule>()
            .Ctor<IAction>().Is(new MySpecialActionInstance());

    }

    public class BigCondition : ICondition
    {
        public BigCondition(int number)
        {
        }

        public bool Matches(SomeEvent @event)
        {
            throw new NotImplementedException();
        }
    }

    public class MySpecialActionInstance : Instance
    {
        public MySpecialActionInstance() : base(typeof(IAction), typeof(IAction), ServiceLifetime.Transient)
        {
        }

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            throw new NotImplementedException();
        }

        public override object Resolve(Scope scope)
        {
            throw new NotImplementedException();
        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            throw new NotImplementedException();
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L113-L173' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-simple-ctor-injection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The inline dependency configuration using the `Ctor<>().Is()` syntax supports all the common Lamar configuration options: define by type, by lambdas, by value, or if you really want to risk severe eye strain, you can use your own Instance objects and define the configuration of your dependency's dependencies.

## Specifying the Argument Name

If for some reason you need to specify an inline constructor argument dependency, and the concrete type has more than one dependency for that type, you just need to specify the parameter name as shown in this sample:

<!-- snippet: sample_inline-dependencies-ctor-by-name -->
<a id='snippet-sample_inline-dependencies-ctor-by-name'></a>
```cs
public class DualConditionRule : IEventRule
{
    private readonly ICondition _first;
    private readonly ICondition _second;
    private readonly IAction _action;

    public DualConditionRule(ICondition first, ICondition second, IAction action)
    {
        _first = first;
        _second = second;
        _action = action;
    }

    public void ProcessEvent(SomeEvent @event)
    {
        if (_first.Matches(@event) || _second.Matches(@event))
        {
            _action.PerformWork(@event);
        }
    }
}

public class DualConditionRuleRegistry : ServiceRegistry
{
    public DualConditionRuleRegistry()
    {
        // In this case, because DualConditionRule
        // has two different
        For<IEventRule>().Use<DualConditionRule>()
            .Ctor<ICondition>("first").Is<Condition1>()
            .Ctor<ICondition>("second").Is<Condition2>();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L175-L210' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-ctor-by-name' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Setter Dependencies

You can also configure setter dependencies with a similar syntax, but with additional options to specify the property name by using an `Expression` as shown below:

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
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L212-L240' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-setters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip INFO
The `Ctor` and `Setter` methods are just syntactic sugar. Both methods store data to the same underlying structure.
:::

## Enumerable Dependencies

TODO(show a sample of using enumerable dependencies)

## Programmatic Configuration outside of the Registry DSL

In some cases, you may want to skip the Registry DSL and go straight for the raw dependencies structures. Let's say thatwe're using an open generic type for our rules engine so that we can respond to multiple event types:

<!-- snippet: sample_inline-dependencies-open-types -->
<a id='snippet-sample_inline-dependencies-open-types'></a>
```cs
public interface IEventRule<TEvent>
{
    void ProcessEvent(TEvent @event);
}

public interface ICondition<TEvent>
{
    bool Matches(TEvent @event);
}

public class Condition1<TEvent> : ICondition<TEvent>
{
    public bool Matches(TEvent @event)
    {
        throw new NotImplementedException();
    }
}

public interface IAction<TEvent>
{
    void PerformWork(TEvent @event);
}

public class Action1<TEvent> : IAction<TEvent>
{
    public void PerformWork(TEvent @event)
    {
        throw new NotImplementedException();
    }
}

public class EventRule<TEvent> : IEventRule<TEvent>
{
    private readonly string _name;
    private readonly ICondition<TEvent> _condition;
    private readonly IAction<TEvent> _action;

    public EventRule(string name, ICondition<TEvent> condition, IAction<TEvent> action)
    {
        _name = name;
        _condition = condition;
        _action = action;
    }

    public string Name
    {
        get { return _name; }
    }

    public void ProcessEvent(TEvent @event)
    {
        if (_condition.Matches(@event))
        {
            _action.PerformWork(@event);
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L242-L301' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-open-types' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

It's frequently useful to explicitly configure all the elements for an enumerable argument (arrays, IEnumerable, or IList).
Lamar provides this syntax to do just that:

<!-- snippet: sample_inline-dependencies-enumerables -->
<a id='snippet-sample_inline-dependencies-enumerables'></a>
```cs
public class BigRule : IEventRule
{
    private readonly IEnumerable<ICondition> _conditions;
    private readonly IEnumerable<IAction> _actions;

    public BigRule(IEnumerable<ICondition> conditions, IEnumerable<IAction> actions)
    {
        _conditions = conditions;
        _actions = actions;
    }

    public void ProcessEvent(SomeEvent @event)
    {
        if (_conditions.Any(x => x.Matches(@event)))
        {
            _actions.Each(x => x.PerformWork(@event));
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/inline_dependencies.cs#L305-L327' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inline-dependencies-enumerables' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
