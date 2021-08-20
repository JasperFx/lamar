# Try Getting an Optional Service by Service Type and Name

Just use the `IContainer.TryGetInstance<T>(name)` or `IContainer.TryGetInstance(Type pluginType, string name)` method as shown below:

<!-- snippet: sample_TryGetInstanceViaNameAndGeneric_ReturnsInstance_WhenTypeFound -->
<a id='snippet-sample_trygetinstancevianameandgeneric_returnsinstance_whentypefound'></a>
```cs
[Fact]
public void TryGetInstanceViaNameAndGeneric_ReturnsInstance_WhenTypeFound()
{
    addColorInstance("Red");
    addColorInstance("Orange");
    addColorInstance("Blue");

    // "Orange" exists, so an object should be returned
    var instance = _container.TryGetInstance<Rule>("Orange");
    instance.ShouldBeOfType(typeof(ColorRule));
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/ContainerTester.cs#L268-L281' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_trygetinstancevianameandgeneric_returnsinstance_whentypefound' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
