# Get a Service by PluginType

Requesting the default configured object of a plugin type is done through the `IContainer.GetInstance()` method shown below:

<!-- snippet: sample_GetInstance -->
<a id='snippet-sample_getinstance'></a>
```cs
[Fact]
public void get_the_default_instance()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Use<AWidget>();
    });

    container.GetInstance<IWidget>()
        .ShouldBeOfType<AWidget>();

    // or

    container.GetInstance(typeof(IWidget))
        .ShouldBeOfType<AWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/get_all_instances.cs#L43-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getinstance' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_getinstance-1'></a>
```cs
[Fact]
public void get_the_default_instance()
{
    var container = new Container(x => { x.For<IWidget>().Use<AWidget>(); });

    container.GetInstance<IWidget>()
        .ShouldBeOfType<AWidget>();

    // or

    container.GetInstance(typeof(IWidget))
        .ShouldBeOfType<AWidget>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/SimpleScenarios.cs#L9-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_getinstance-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
