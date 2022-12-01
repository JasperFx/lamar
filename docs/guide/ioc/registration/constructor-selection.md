# Constructor Selection

Lamar's constructor selection logic is compliant with the ASP.Net Core DI specifications and uses their definition of selecting the "greediest constructor." Definitely note that **this behavior is different than StructureMap's version of "greediest constructor" selection**.

Constructor selection can be happily overridden by using one of the mechanisms shown below or using custom [instance policies](/guide/ioc/registration/policies).

## Greediest Constructor

If there are multiple public constructor functions on a concrete class, Lamar's default behavior is to select the "greediest" constructor where Lamar can resolve all of the parameters, i.e., the constructor function with the most parameters. In the case of two or more constructor functions with the same number of parameters Lamar will simply take the first constructor encountered in that subset of constructors assuming all the constructor parameter lists can be filled by the container.

The default constructor selection is demonstrated below:

<!-- snippet: sample_select-the-greediest-ctor -->
<a id='snippet-sample_select-the-greediest-ctor'></a>
```cs
public class GreaterThanRule : Rule
{
    public string Attribute { get; set; }
    public int Value { get; set; }

    public GreaterThanRule()
    {
    }

    public GreaterThanRule(string attribute, int value)
    {
        Attribute = attribute;
        Value = value;
    }

    public GreaterThanRule(IWidget widget, Rule rule)
    {
    }
}

[Fact]
public void using_the_greediest_ctor()
{
    var container = new Container(_ =>
    {
        _.ForConcreteType<GreaterThanRule>().Configure
            .Ctor<string>("attribute").Is("foo")
            .Ctor<int>("value").Is(42);
    });

    var rule = container.GetInstance<GreaterThanRule>();
    rule.Attribute.ShouldBe("foo");
    rule.Value.ShouldBe(42);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Pipeline/ConstructorSelectorTester.cs#L26-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_select-the-greediest-ctor' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The "greediest constructor selection" will bypass any constructor function that requires "simple" arguments like strings, numbers, or enumeration values that are not explicitly configured for the instance.

You can see this behavior shown below:

<!-- snippet: sample_skip-ctor-with-missing-simples -->
<a id='snippet-sample_skip-ctor-with-missing-simples'></a>
```cs
public class DbContext
{
    public string ConnectionString { get; set; }

    public DbContext(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public DbContext() : this("default value")
    {
    }
}

[Fact]
public void should_bypass_ctor_with_unresolvable_simple_args()
{
    var container = Container.Empty();
    container.GetInstance<DbContext>()
        .ConnectionString.ShouldBe("default value");
}

[Fact]
public void should_use_greediest_ctor_that_has_all_of_simple_dependencies()
{
    var container = new Container(_ =>
    {
        _.ForConcreteType<DbContext>().Configure
            .Ctor<string>("connectionString").Is("not the default");
    });

    container.GetInstance<DbContext>()
        .ConnectionString.ShouldBe("not the default");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/constructor_selection.cs#L127-L163' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_skip-ctor-with-missing-simples' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_skip-ctor-with-missing-simples-1'></a>
```cs
public class DbContext
{
    public string ConnectionString { get; set; }

    public DbContext(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public DbContext() : this("default value")
    {
    }
}

[Fact]
public void should_bypass_ctor_with_unresolvable_simple_args()
{
    var container = new Container();
    container.GetInstance<DbContext>()
        .ConnectionString.ShouldBe("default value");
}

[Fact]
public void should_use_greediest_ctor_that_has_all_of_simple_dependencies()
{
    var container = new Container(_ =>
    {
        _.ForConcreteType<DbContext>().Configure
            .Ctor<string>("connectionString").Is("not the default");
    });

    container.GetInstance<DbContext>()
        .ConnectionString.ShouldBe("not the default");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/constructor_selection.cs#L158-L194' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_skip-ctor-with-missing-simples-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Explicitly Selecting a Constructor

To override the constructor selection explicitly on a case by case basis, you
can use the `SelectConstructor(Expression)` method in the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) as shown below:

<!-- snippet: sample_explicit-ctor-selection -->
<a id='snippet-sample_explicit-ctor-selection'></a>
```cs
public class Thingie
{
    public Thingie(IWidget widget)
    {
        CorrectCtorWasUsed = true;
    }

    public bool CorrectCtorWasUsed { get; set; }

    public Thingie(IWidget widget, IService service)
    {
        Assert.True(false, "I should not have been called");
    }
}

[Fact]
public void override_the_constructor_selection()
{
    var container = new Container(_ =>
    {
        _.For<IWidget>().Use<AWidget>();

        _.ForConcreteType<Thingie>().Configure

            // StructureMap parses the expression passed
            // into the method below to determine the
            // constructor
            .SelectConstructor(() => new Thingie(null));
    });

    container.GetInstance<Thingie>()
        .CorrectCtorWasUsed
        .ShouldBeTrue();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/constructor_selection.cs#L89-L125' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_explicit-ctor-selection' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_explicit-ctor-selection-1'></a>
```cs
public class Thingie
{
    public Thingie(IWidget widget)
    {
        CorrectCtorWasUsed = true;
    }

    public bool CorrectCtorWasUsed { get; set; }

    public Thingie(IWidget widget, IService service)
    {
        Assert.True(false, "I should not have been called");
    }
}

[Fact]
public void override_the_constructor_selection()
{
    var container = new Container(_ =>
    {
        _.For<IWidget>().Use<AWidget>();

        _.ForConcreteType<Thingie>().Configure

            // StructureMap parses the expression passed
            // into the method below to determine the
            // constructor
            .SelectConstructor(() => new Thingie(null));
    });

    container.GetInstance<Thingie>()
        .CorrectCtorWasUsed
        .ShouldBeTrue();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/constructor_selection.cs#L120-L156' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_explicit-ctor-selection-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## [DefaultConstructor] Attribute

Alternatively, you can override the choice of constructor function by using the older `[DefaultConstructor]` attribute like this:

<!-- snippet: sample_using-default-ctor-attribute -->
<a id='snippet-sample_using-default-ctor-attribute'></a>
```cs
public class AttributedThing
{
    // Normally the greediest ctor would be
    // selected, but using this attribute
    // will overrid that behavior
    [DefaultConstructor]
    public AttributedThing(IWidget widget)
    {
        CorrectCtorWasUsed = true;
    }

    public bool CorrectCtorWasUsed { get; set; }

    public AttributedThing(IWidget widget, IService service)
    {
        Assert.True(false, "I should not have been called");
    }
}

[Fact]
public void select_constructor_by_attribute()
{
    var container = new Container(_ => { _.For<IWidget>().Use<AWidget>(); });

    container.GetInstance<AttributedThing>()
        .CorrectCtorWasUsed
        .ShouldBeTrue();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/constructor_selection.cs#L57-L87' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-default-ctor-attribute' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_using-default-ctor-attribute-1'></a>
```cs
public class AttributedThing
{
    // Normally the greediest ctor would be
    // selected, but using this attribute
    // will overrid that behavior
    [DefaultConstructor]
    public AttributedThing(IWidget widget)
    {
        CorrectCtorWasUsed = true;
    }

    public bool CorrectCtorWasUsed { get; set; }

    public AttributedThing(IWidget widget, IService service)
    {
        Assert.True(false, "I should not have been called");
    }
}

[Fact]
public void select_constructor_by_attribute()
{
    var container = new Container(_ => { _.For<IWidget>().Use<AWidget>(); });

    container.GetInstance<AttributedThing>()
        .CorrectCtorWasUsed
        .ShouldBeTrue();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/constructor_selection.cs#L88-L118' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-default-ctor-attribute-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
