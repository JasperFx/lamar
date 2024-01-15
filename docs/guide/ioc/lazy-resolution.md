# Lazy Resolution

Lamar has some built in functionality for "lazy" resolved dependencies, so that instead of your
application service taking a direct dependency on `IExpensiveToBuildService` that might not be necessary,
you could instead have Lamar fulfil a dependency on `Lazy<IExpensiveToBuildService>` or `Func<IExpensiveToBuildService>`
that could be used to retrieve that expensive service only when it is needed from whatever `Container` originally created
the parent object.

Do note that the `Lazy<T>` and `Func<T>` approaches respect the lifecycle of the underlying registration rather than
automatically building a unique object instance.

Also note that `Lazy<T>` or `Func<T>` is your best (only) viable approach if you wish to have Lamar inject bi-directional
relationships.

## Lazy\<T\>

Assuming that Lamar either has an existing configuration for `T` or can
derive a way to build `T`, you can just declare a dependency on `Lazy<T>` like this sample:

<!-- snippet: sample_Lazy-in-usage -->
<a id='snippet-sample_lazy-in-usage'></a>
```cs
public class WidgetLazyUser
{
    private readonly Lazy<IWidget> _widget;

    public WidgetLazyUser(Lazy<IWidget> widget)
    {
        _widget = widget;
    }

    public IWidget Widget => _widget.Value;
}

[Fact]
public void lazy_resolution_in_action()
{
    var container = new Container(_ => { _.For<IWidget>().Use<AWidget>(); });

    container.GetInstance<WidgetLazyUser>()
        .Widget.ShouldBeOfType<AWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/lazy_and_func_resolution.cs#L180-L203' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_lazy-in-usage' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_lazy-in-usage-1'></a>
```cs
public class WidgetLazyUser
{
    private readonly Lazy<IWidget> _widget;

    public WidgetLazyUser(Lazy<IWidget> widget)
    {
        _widget = widget;
    }

    public IWidget Widget
    {
        get { return _widget.Value; }
    }
}

[Fact]
public void lazy_resolution_in_action()
{
    var container = new Container(_ =>
    {
        _.For<IWidget>().Use<AWidget>();
    });

    container.GetInstance<WidgetLazyUser>()
        .Widget.ShouldBeOfType<AWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Pipeline/Lazy_and_Func_construction_strategy_Tester.cs#L21-L49' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_lazy-in-usage-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Func&lt;T&gt;

Likewise, you can also declare a dependency on `Func<T>` with very similar mechanics:

<!-- snippet: sample_using-func-t -->
<a id='snippet-sample_using-func-t'></a>
```cs
[Fact]
public void build_a_func_that_returns_a_singleton()
{
    var container = new Container(x => { x.ForSingletonOf<IWidget>().Use<AWidget>(); });

    var func = container.GetInstance<Func<IWidget>>();
    var w1 = func();
    var w2 = func();
    var w3 = func();

    w1.ShouldBeOfType<AWidget>();

    w1.ShouldBeSameAs(w2);
    w1.ShouldBeSameAs(w3);
    w2.ShouldBeSameAs(w3);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/lazy_and_func_resolution.cs#L67-L86' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-func-t' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-func-t-1'></a>
```cs
[Fact]
public void build_a_func_that_returns_a_singleton()
{
    var container = new Container(x =>
    {
        x.ForSingletonOf<IWidget>().Use<ColorWidget>().Ctor<string>("color").Is("green");
    });

    var func = container.GetInstance<Func<IWidget>>();
    var w1 = func();
    var w2 = func();
    var w3 = func();

    w1.ShouldBeOfType<ColorWidget>().Color.ShouldBe("green");

    w1.ShouldBeTheSameAs(w2);
    w1.ShouldBeTheSameAs(w3);
    w2.ShouldBeTheSameAs(w3);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Pipeline/Lazy_and_Func_construction_strategy_Tester.cs#L85-L106' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-func-t-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

_This functionality predates the introduction of the Lazy type to .Net_

## Func\<string, T\>

Finally, you can also declare a dependency on `Func<string, T>` that will allow you to lazily
resolve a dependency of `T` by name:

<!-- snippet: sample_using-func-string-t -->
<a id='snippet-sample_using-func-string-t'></a>
```cs
[Fact]
public void build_a_func_by_string()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Add<GreenWidget>().Named("green");
        x.For<IWidget>().Add<BlueWidget>().Named("blue");
        x.For<IWidget>().Add<RedWidget>().Named("red");
    });

    var func = container.GetInstance<Func<string, IWidget>>();
    func("green").ShouldBeOfType<GreenWidget>();
    func("blue").ShouldBeOfType<BlueWidget>();
    func("red").ShouldBeOfType<RedWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/lazy_and_func_resolution.cs#L88-L106' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-func-string-t' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-func-string-t-1'></a>
```cs
[Fact]
public void build_a_func_by_string()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Add<ColorWidget>().Ctor<string>("color").Is("green").Named("green");
        x.For<IWidget>().Add<ColorWidget>().Ctor<string>("color").Is("blue").Named("blue");
        x.For<IWidget>().Add<ColorWidget>().Ctor<string>("color").Is("red").Named("red");
    });

    var func = container.GetInstance<Func<string, IWidget>>();
    func("green").ShouldBeOfType<ColorWidget>().Color.ShouldBe("green");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Pipeline/Lazy_and_Func_construction_strategy_Tester.cs#L108-L123' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-func-string-t-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Bi-relational Dependency Workaround

**Lamar does not directly support bi-directional dependency relationships** -- but will happily tell you in an exception when
you accidentally manage to create one without cratering your AppDomain with a `StackOverflowException`.

Either `Func<T>` or `Lazy<T>` can be used as a workaround for purposeful bi-directional dependencies between types. The
following is an example of using this strategy:

<!-- snippet: sample_using-lazy-as-workaround-for-bidirectional-dependency -->
<a id='snippet-sample_using-lazy-as-workaround-for-bidirectional-dependency'></a>
```cs
public class Thing1
{
    private readonly Lazy<Thing2> _thing2;

    public Thing1(Lazy<Thing2> thing2)
    {
        _thing2 = thing2;
    }

    public Thing2 Thing2 => _thing2.Value;
}

public class Thing2
{
    public Thing2(Thing1 thing1)
    {
        Thing1 = thing1;
    }

    public Thing1 Thing1 { get; set; }
}

[Fact]
public void use_lazy_as_workaround_for_bi_directional_dependency()
{
    var container = Container.For(_ =>
    {
        _.AddSingleton<Thing1>();
        _.AddSingleton<Thing2>();
    });

    var thing1 = container.GetInstance<Thing1>();
    var thing2 = container.GetInstance<Thing2>();

    thing1.Thing2.ShouldBeSameAs(thing2);
    thing2.Thing1.ShouldBeSameAs(thing1);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/lazy_and_func_resolution.cs#L205-L245' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-lazy-as-workaround-for-bidirectional-dependency' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-lazy-as-workaround-for-bidirectional-dependency-1'></a>
```cs
[Singleton]
public class Thing1
{
    private readonly Lazy<Thing2> _thing2;

    public Thing1(Lazy<Thing2> thing2)
    {
        _thing2 = thing2;
    }

    public Thing2 Thing2
    {
        get { return _thing2.Value; }
    }
}

[Singleton]
public class Thing2
{
    public Thing1 Thing1 { get; set; }

    public Thing2(Thing1 thing1)
    {
        Thing1 = thing1;
    }
}

[Fact]
public void use_lazy_as_workaround_for_bi_directional_dependency()
{
    var container = new Container();
    var thing1 = container.GetInstance<Thing1>();
    var thing2 = container.GetInstance<Thing2>();

    thing1.Thing2.ShouldBeSameAs(thing2);
    thing2.Thing1.ShouldBeSameAs(thing1);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Pipeline/Lazy_and_Func_construction_strategy_Tester.cs#L171-L211' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-lazy-as-workaround-for-bidirectional-dependency-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
