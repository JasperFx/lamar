# Try Getting an Optional Service by Plugin Type and Name

Just use the `IContainer.TryGetInstance<T>(name)` or `IContainer.TryGetInstance(Type pluginType, string name)` method as shown below:

<[sample:TryGetInstanceViaNameAndGeneric_ReturnsInstance_WhenTypeFound]>
