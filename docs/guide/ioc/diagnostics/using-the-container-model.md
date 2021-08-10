# Using the Container Model

The queryable `Container.Model` is a power facility to look into your Lamar `Container` and even to eject previously built services from the Container. The `Container.Model` represents a **static view of what a `Container` already has**, and does not account for policies that may allow Lamar to validly discover types that it may encounter later at runtime.

## Querying for Plugin Types

The [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) mechanism works by just iterating over the `Container.Model.PluginTypes` collection as shown below:

<[sample:find-all-plugin-types-from-the-current-assembly]>

## Working with a Service Type

The `IServiceFamilyConfiguration` interface allows you to query and manipulate all the configured Instance's for a given plugin type.

See the entire [IServiceFamilyConfiguration interface here](https://github.com/JasperFx/lamar/blob/master/src/Lamar/IServiceFamilyConfiguration.cs).

## Finding the Default for a Service Type

To simply find out what the default concrete type would be for a requested service type, use one of these two methods:

<[sample:find-default-of-plugintype]>

## Finding an Instance by Type and Name

Use this syntax to find information about an Instance in a given service type and name:

<[sample:find-named-instance-by-type-and-name]>

## All Instances for a Service Type

This sample shows how you could iterate or query over all the registered instances for a single service type:

<[sample:query-instances-of-plugintype]>

## Testing for Registrations

To troubleshoot or automate testing of Container configuration, you can use code like the sample below to
test for the presence of expected configurations:

<[sample:testing-for-registrations]>

## Finding all Possible Implementors of an Interface

Forget about what types are registered for whatever service types and consider this, what if you have an interface called
`IStartable` that just denotes objects that will need to be activated after the container is bootstrapped?

If our interface is this below:

<[sample:istartable]>

We could walk through the entire Lamar model and find every registered instance that implements this interface, create each, and call the `Start()` method like in this code below:

<[sample:calling-startable-start]>

I've also used this mechanism in automated testing to reach out to all singleton services that may have state to clear out their data between tests.
