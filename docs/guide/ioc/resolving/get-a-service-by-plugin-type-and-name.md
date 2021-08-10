# Get a Service by Plugin Type and Name

You can also request a named configuration for a given service type by using the overloads of `IContainer.GetInstance()` that take in a name like this:

<!-- snippet: sample_GetInstance-by-name -->
<a id='snippet-sample_getinstance-by-name'></a>
```cs
[Fact]
public void get_a_named_instance()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Add<AWidget>().Named("A");
        x.For<IWidget>().Add<BWidget>().Named("B");
        x.For<IWidget>().Add<CWidget>().Named("C");
    });

    container.GetInstance<IWidget>("A").ShouldBeOfType<AWidget>();
    container.GetInstance<IWidget>("B").ShouldBeOfType<BWidget>();
    container.GetInstance<IWidget>("C").ShouldBeOfType<CWidget>();

    // or

    container.GetInstance(typeof(IWidget), "A").ShouldBeOfType<AWidget>();
    container.GetInstance(typeof(IWidget), "B").ShouldBeOfType<BWidget>();
    container.GetInstance(typeof(IWidget), "C").ShouldBeOfType<CWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/get_all_instances.cs#L63-L85' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getinstance-by-name' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_getinstance-by-name-1'></a>
```cs
[Fact]
public void get_a_named_instance()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Add<AWidget>().Named("A");
        x.For<IWidget>().Add<BWidget>().Named("B");
        x.For<IWidget>().Add<CWidget>().Named("C");
    });

    container.GetInstance<IWidget>("A").ShouldBeOfType<AWidget>();
    container.GetInstance<IWidget>("B").ShouldBeOfType<BWidget>();
    container.GetInstance<IWidget>("C").ShouldBeOfType<CWidget>();

    // or

    container.GetInstance(typeof(IWidget), "A").ShouldBeOfType<AWidget>();
    container.GetInstance(typeof(IWidget), "B").ShouldBeOfType<BWidget>();
    container.GetInstance(typeof(IWidget), "C").ShouldBeOfType<CWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/SimpleScenarios.cs#L26-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getinstance-by-name-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
