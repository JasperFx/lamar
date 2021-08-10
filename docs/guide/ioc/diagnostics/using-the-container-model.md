# Using the Container Model

The queryable `Container.Model` is a power facility to look into your Lamar `Container` and even to eject previously built services from the Container. The `Container.Model` represents a **static view of what a `Container` already has**, and does not account for policies that may allow Lamar to validly discover types that it may encounter later at runtime.

## Querying for Plugin Types

The [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) mechanism works by just iterating over the `Container.Model.PluginTypes` collection as shown below:

<!-- snippet: sample_find-all-plugin-types-from-the-current-assembly -->
<a id='snippet-sample_find-all-plugin-types-from-the-current-assembly'></a>
```cs
container.Model.PluginTypes.Where(x => x.PluginType.Assembly == Assembly.GetExecutingAssembly())
    .Each(pluginType => Debug.WriteLine(pluginType.PluginType));
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/UsingContainerModel.cs#L19-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_find-all-plugin-types-from-the-current-assembly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Working with a Service Type

The `IServiceFamilyConfiguration` interface allows you to query and manipulate all the configured Instance's for a given plugin type.

See the entire [IServiceFamilyConfiguration interface here](https://github.com/JasperFx/lamar/blob/master/src/Lamar/IServiceFamilyConfiguration.cs).

## Finding the Default for a Service Type

To simply find out what the default concrete type would be for a requested service type, use one of these two methods:

<!-- snippet: sample_find-default-of-plugintype -->
<a id='snippet-sample_find-default-of-plugintype'></a>
```cs
// Finding the concrete type of the default
// IDevice service
container.Model.DefaultTypeFor<IDevice>()
    .ShouldBe(typeof(DefaultDevice));

// Find the configuration model for the default
// IDevice service
container.Model.For<IDevice>().Default
    .ReturnedType.ShouldBe(typeof(DefaultDevice));
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/UsingContainerModel.cs#L24-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_find-default-of-plugintype' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Finding an Instance by Type and Name

Use this syntax to find information about an Instance in a given service type and name:

<!-- snippet: sample_find-named-instance-by-type-and-name -->
<a id='snippet-sample_find-named-instance-by-type-and-name'></a>
```cs
var redRule = container.Model.Find<Rule>("Red");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/UsingContainerModel.cs#L36-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_find-named-instance-by-type-and-name' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## All Instances for a Service Type

This sample shows how you could iterate or query over all the registered instances for a single service type:

<!-- snippet: sample_query-instances-of-plugintype -->
<a id='snippet-sample_query-instances-of-plugintype'></a>
```cs
container.Model.For<Rule>().Instances.Each(i =>
{
    Debug.WriteLine(i.Instance.Description);
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/UsingContainerModel.cs#L40-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_query-instances-of-plugintype' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Testing for Registrations

To troubleshoot or automate testing of Container configuration, you can use code like the sample below to
test for the presence of expected configurations:

<!-- snippet: sample_testing-for-registrations -->
<a id='snippet-sample_testing-for-registrations'></a>
```cs
// Is there a default instance for IDevice?
container.Model.HasDefaultImplementationFor<IDevice>().ShouldBeTrue();

// Are there any configured instances for IDevice?
container.Model.HasImplementationsFor<IDevice>().ShouldBeTrue();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/UsingContainerModel.cs#L67-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_testing-for-registrations' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Finding all Possible Implementors of an Interface

Forget about what types are registered for whatever service types and consider this, what if you have an interface called
`IStartable` that just denotes objects that will need to be activated after the container is bootstrapped?

If our interface is this below:

<!-- snippet: sample_istartable -->
<a id='snippet-sample_istartable'></a>
```cs
public interface IStartable
{
    bool WasStarted { get; }

    void Start();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/container_model_usage.cs#L267-L275' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_istartable' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_istartable-1'></a>
```cs
public interface IStartable
{
    bool WasStarted { get; }

    void Start();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Query/ModelIntegrationTester.cs#L188-L196' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_istartable-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

We could walk through the entire Lamar model and find every registered instance that implements this interface, create each, and call the `Start()` method like in this code below:

<!-- snippet: sample_calling-startable-start -->
<a id='snippet-sample_calling-startable-start'></a>
```cs
var allStartables = container.Model.GetAllPossible<IStartable>();
allStartables.ToArray()
    .Each(x => x.Start());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/container_model_usage.cs#L219-L223' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_calling-startable-start' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_calling-startable-start-1'></a>
```cs
var allStartables = container.Model.GetAllPossible<IStartable>();
allStartables.ToArray()
    .Each(x => x.Start());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Query/ModelIntegrationTester.cs#L102-L106' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_calling-startable-start-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

I've also used this mechanism in automated testing to reach out to all singleton services that may have state to clear out their data between tests.
