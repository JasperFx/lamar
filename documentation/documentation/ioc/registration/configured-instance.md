<!--Title:Working with IConfiguredInstance-->

The most common way for StructureMap to build or resolve a requested object is to build a concrete type directly by calling a
public constructor function and optionally filling values in public setter properties. For this type of object construction, 
StructureMap exposes the `IConfiguredInstance` interface as a means of querying and modifying how a concrete type will be
created or resolved. While the <[linkto:documentation/ioc/registration/registry-dsl]> fluent interface provides the main way of explicitly configuring concrete type creation,
the `IConfiguredInstance` interface is meant to support <[linkto:documentation/ioc/registration/auto-registration-and-conventions;title=conventional registration]>, 
<[linkto:documentation/ioc/registration/attributes;title=configuration attributes]>, and <[linkto:documentation/ioc/registration/policies;title=construction policies]>.

<[sample:IConfiguredInstance]>


## Changing the Instance Lifecycle

You can override the lifecycle of a single `IConfiguredInstance` by calling the `LifecycleIs()` methods and either supplying a 
type of `ILifecycle` or an `ILifecycle` object. As a quick helper, there are also extension methods for common lifecycles:

<[sample:iconfiguredinstance-lifecycle]>

## Reflecting over Constructor Parameters

To find the constructor function parameters of an `IConfiguredInstance`, just use this syntax (it's just .Net Reflection):

<[sample:reflecting-over-parameters]>

**The <[linkto:documentation/ioc/registration/constructor-selection;title=constructor function selection]> process takes place as the very first step in creating a <[linkto:documentation/ioc/diagnostics/build-plans;title=build plan]> and will be
available in any kind of <[linkto:documentation/ioc/registration/policies;title=construction policy]> or <[linkto:documentation/ioc/registration/attributes;title=configuration attribute]> on
parameters or properties.**


## Working with Inline Dependencies

The `IConfiguredInstance.InlineDependencies` property is a collection of `Instance` objects that model inline dependencies. A
single _Instance_ refers to a parameter in a constructor function:

When Lamar determines a <[linkto:documentation/ioc/diagnostics/build-plans;title=build plan]> for a concrete type, it reflects over all the 
parameters in the chosen constructor function and then the settable properties looking for any explicitly configured
dependencies by searching in order for:

1. An exact match by dependency type and name
1. A partial match by dependency type only
1. A partial match by name only

For primitive arguments like strings or numbers, the logic is to search first by name, then by type. All searching is done in
the order that the inline `Instance` objects are registered, so do watch the order in which you add arguments. There is a method to
insert new arguments at the front of the list if you need to do any kind of overrides of previous behavior.

See <[linkto:documentation/ioc/registration/inline-dependencies]> for more information about working with inline dependencies.
