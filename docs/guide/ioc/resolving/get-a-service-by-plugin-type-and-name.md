# Get a Service by Service Type and Name

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
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/get_all_instances.cs#L44-L67' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getinstance-by-name' title='Start of snippet'>anchor</a></sup>
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

## .NET Core Keyed Services

While Lamar (and StructureMap before that) has supported the idea of named service registrations in production applications since 2004 (!),
.NET finally discovered this usage in .NET 8. Lamar 12.1.0 introduces native support for [keyed services](https://weblogs.asp.net/ricardoperes/net-8-dependency-injection-changes-keyed-services) according to the new .NET standard as shown below:

<!-- snippet: sample_adding_keyed_services -->
<a id='snippet-sample_adding_keyed_services'></a>
```cs
var container = Container.For(services =>
{
    
    services.AddKeyedSingleton<IWidget, AWidget>("one");
    services.AddKeyedScoped<IWidget>("two", (_, _) => new BWidget());
    services.AddKeyedSingleton<IWidget>("three", new CWidget());

    services.AddKeyedSingleton<CWidget>("C1");
    services.AddKeyedSingleton<CWidget>("C2");
    services.AddKeyedSingleton<CWidget>("C3");
});

container.GetInstance<IWidget>("one").ShouldBeOfType<AWidget>();
container.GetKeyedService<IWidget>("one").ShouldBeOfType<AWidget>();

container.GetInstance<IWidget>("two").ShouldBeOfType<BWidget>();
container.GetKeyedService<IWidget>("two").ShouldBeOfType<BWidget>();

container.GetInstance<IWidget>("three").ShouldBeOfType<CWidget>();
container.GetKeyedService<IWidget>("three").ShouldBeOfType<CWidget>();

container.GetInstance<CWidget>("C1").ShouldBeOfType<CWidget>();
container.GetKeyedService<CWidget>("C2").ShouldBeOfType<CWidget>();

container.GetKeyedService<CWidget>("C2")
    .ShouldBeSameAs(container.GetKeyedService<CWidget>("C2"));

container.GetKeyedService<CWidget>("C2")
    .ShouldNotBeSameAs(container.GetKeyedService<CWidget>("C3"));
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/IKeyedServiceProvider_compliance.cs#L15-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_adding_keyed_services' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
