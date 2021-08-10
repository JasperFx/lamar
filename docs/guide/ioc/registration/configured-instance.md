# Working with IConfiguredInstance

  The most common way for StructureMap to build or resolve a requested object is to build a concrete type directly by calling a public constructor function and optionally filling values in public setter properties. For this type of object construction, StructureMap exposes the `IConfiguredInstance` interface as a means of querying and modifying how a concrete type will be created or resolved. While the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) fluent interface provides the main way of explicitly configuring concrete type creation, the `IConfiguredInstance` interface is meant to support [conventional registration](/guide/ioc/registration/auto-registration-and-conventions), [configuration attributes](/guide/ioc/registration/attributes), and [construction policies](/guide/ioc/registration/policies).

<[sample:IConfiguredInstance]>

## Changing the Instance Lifecycle

You can override the lifecycle of a single `IConfiguredInstance` by calling the `LifecycleIs()` methods and either supplying a type of `ILifecycle` or an `ILifecycle` object. As a quick helper, there are also extension methods for common lifecycles:

<[sample:iconfiguredinstance-lifecycle]>

## Reflecting over Constructor Parameters

To find the constructor function parameters of an `IConfiguredInstance`, just use this syntax (it's just .Net Reflection):

<[sample:reflecting-over-parameters]>

::: tip INFO
The [constructor function selection](/guide/ioc/registration/constructor-selection) process takes place as the very first step in creating a [build plan](/guide/ioc/diagnostics/build-plans) and will be available in any kind of [construction policy](/guide/ioc/registration/policies) or [configuration attribute](/guide/ioc/registration/attributes) on parameters or properties.
:::

## Working with Inline Dependencies

The `IConfiguredInstance.InlineDependencies` property is a collection of `Instance` objects that model inline dependencies. A single _Instance_ refers to a parameter in a constructor function:

When Lamar determines a [build plan](/guide/ioc/diagnostics/build-plans) for a concrete type, it reflects over all the parameters in the chosen constructor function and then the settable properties looking for any explicitly configured dependencies by searching in order for:

1. An exact match by dependency type and name
1. A partial match by dependency type only
1. A partial match by name only

For primitive arguments like strings or numbers, the logic is to search first by name, then by type. All searching is done in the order that the inline `Instance` objects are registered, so do watch the order in which you add arguments. There is a method to insert new arguments at the front of the list if you need to do any kind of overrides of previous behavior.

See [inline dependencies](/guide/ioc/registration/inline-dependencies) for more information about working with inline dependencies.
