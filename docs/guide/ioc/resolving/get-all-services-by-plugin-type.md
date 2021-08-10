# Get all Services by Plugin Type

::: warning
The functionality respects the order in which the actual instances are configured in the Container -- which is compliant with the
expected behavior inside of ASP.Net Core.  Be warned that some other IoC tools make different assumptions if you are coming from a different tool.
:::

Please see [working with Enumerable Types](/guide/ioc/working-with-enumerable-types) for a lot more information about what's going on behind the
scenes.

Once in a while you might want to get an enumerable of all the configured objects for a PluginType.  That's done with the `GetAllInstances()` method shown below:

<!-- snippet: sample_get-all-instances -->
<a id='snippet-sample_get-all-instances'></a>
```cs
[Fact]
public void get_all_instances()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Add<AWidget>().Named("A");
        x.For<IWidget>().Add<BWidget>().Named("B");
        x.For<IWidget>().Add<CWidget>().Named("C");
    });
    
    container.QuickBuildAll<IWidget>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    container.GetAllInstances<IWidget>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    // or

    container.GetAllInstances(typeof(IWidget))
        .OfType<IWidget>() // returns an IEnumerable, so I'm casting here
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/get_all_instances.cs#L87-L114' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_get-all-instances' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_get-all-instances-1'></a>
```cs
[Fact]
public void get_all_instances()
{
    var container = new Container(x =>
    {
        x.For<IWidget>().Add<AWidget>().Named("A");
        x.For<IWidget>().Add<BWidget>().Named("B");
        x.For<IWidget>().Add<CWidget>().Named("C");
    });

    container.GetAllInstances<IWidget>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    // or

    container.GetAllInstances(typeof(IWidget))
        .OfType<IWidget>() // returns an IEnumerable, so I'm casting here
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/SimpleScenarios.cs#L50-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_get-all-instances-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
