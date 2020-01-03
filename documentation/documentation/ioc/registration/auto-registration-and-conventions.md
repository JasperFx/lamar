<!--Title: Auto-Registration and Conventions-->
<!--Url: auto-registration-and-conventions-->


Lamar has rich support for registering types by scanning assemblies and applying conventional registrations.
Between scanning and default conventions, configurations are often just a few
lines.


Also see <[linkto:documentation/ioc/diagnostics/type-scanning]> for help in understanding the assembly scanning behavior in your system.


## ServiceRegistry.Scan()

Assembly scanning operations are defined by the `ServiceRegistry.Scan()` method demonstrated below:

<[sample:BasicScanning]>

Please note (because I've been asked this several times over the years) that each call to `ServiceRegistry.Scan()` is an entirely atomic operation that has no impact on previous or subsequent calls.

Any given call to `ServiceRegistry.Scan()` consists of three different things:

1. One or more assemblies to scan for types
1. One or more registration conventions
1. Optionally, set filters to only include certain types or exclude other types from being processed by the scanning operation



## Scan the Calling Assembly

<[warning]>
If you are having any issues with the type scanning using the `TheCallingAssembly()` method, just replace that call with an explicit `Assembly("assembly name")` or `AssemblyContainingType<T>()` call instead of relying on Lamar to walk up the call stack to determine the entry assembly. 
<[/warning]>

One of the easiest ways to register types is by scanning the assembly your
registry is placed in. 

**Note** if you have other registries, Lamar will not automatically
find them.

<[sample:scan-calling-assembly]>


## Search for Assemblies on the File System

Lamar provides facilities for registering types by finding assemblies in the application bin path:

<[sample:scan-filesystem]>

Do note that Lamar 4.0 does not search for `.exe` files in the assembly search. The Lamar team felt this was
problematic and "nobody would ever actually want to do that." We were wrong, and due to many user requests, you can now
**opt in** to scanning `.exe` files with a new public method on `AssemblyScanner` shown below:

<[sample:scan-filesystem-for-exe]>

Do be aware that while this technique is very powerful for extensibility, it's been extremely problematic for
some folks in the past. The Lamar team's recommendation for using this feature is to:

1. Make sure you have some kind of filter on the assemblies scanned for performance and predictability reasons. Either a naming convention or filter
   by an assembly attribute to narrow where Lamar looks
1. Get familiar with the new <[linkto:documentation/ioc/diagnostics/type-scanning;title=type scanning diagnostics]> introduced in 4.0;-)


Behind the scenes, Lamar is using the `Assembly.GetExportedTypes()` method from the .Net CLR to find types and this
mechanism is **very** sensitive to missing dependencies. Again, thanks to the new <[linkto:documentation/ioc/diagnostics/type-scanning;title=type scanning diagnostics]>,
you now have some visibility into assembly loading failures that used to be silently swallowed internally.



## Excluding Types

Lamar also makes it easy to exclude types, either individually or by namespace.
The following examples also show how Lamar can register an assembly by providing
a type within that assembly.

Excluding additional types or namespaces is as easy as calling the corresponding method
again.

<[sample:scan-exclusions]>

You can also ignore specific types through an attribute:

<[sample:using-LamarIgnore]>

## Custom Registration Conventions

It's just not possible (or desirable) for Lamar to include every possible type of auto registration
convention users might want, but that's okay because Lamar allows you to create and use your own
conventions through the `IRegistrationConvention` interface:

<[sample:IRegistrationConvention]>

Let's say that you'd like a custom convention that just registers a concrete type against all the interfaces
that it implements. You could then build a custom `IRegistrationConvention` class like the following example: 

<[sample:custom-registration-convention]>

## The Default ISomething/Something Convention

_The Lamar team contains some VB6 veterans who hate Hungarian Notation, but can't shake the "I" nomenclature._

The "default" convention simply tries to connect concrete classes to interfaces using
the I[Something]/[Something] naming convention as shown in this sample:

<[sample:WithDefaultConventions]>

By default, the behavior is to add new registrations regardless of the existing state of the existing `ServiceRegistry`. In cases where you use a mix of explicit and conventional registrations, or other cases where you use multiple `Scan()` operations, you can control the additive registration behavior as shown below:

<[sample:WithDefaultConventionsOptions]>

<[info]>
The service lifetime override behavior was added in Lamar v4.1
<[/info]>

Lastly, you can change the default `Lifetime` of the discovered registrations like this:

<sample:WithDefaultConventionsLifetime>

Otherwise, the default registration will be `Lifetime.Transient`.

## Registering the Single Implementation of an Interface

To tell Lamar to automatically register any interface that only has one concrete implementation, use this method:

<[sample:SingleImplementationsOfInterface]>

## Register all Concrete Types of an Interface

To add all concrete types that can be cast to a named plugin type, use this syntax:

<[sample:register-all-types-implementing]>

**Note, "T" does not have to be an interface, it's all based on the ability to cast a concrete type to the "T"**


## Generic Types

See <[linkto:documentation/ioc/generics]> for an example of using the `ConnectImplementationsToTypesClosing`
mechanism for generic types.


## Register Concrete Types against the First Interface

The last built in registration convention is a mechanism to register all concrete types
that implement at least one interface against the first interface that they implement.

<[sample:using-RegisterConcreteTypesAgainstTheFirstInterface]>


## Look for Registries

<[warning]>
Use some caution with this feature. Many users tried to use this feature with StructureMap just to try 
to break direct assembly coupling, and while it does accomplish that goal, ask yourself if the extra complexity is worth it.
<[/warning]>

Lamar can automatically include other registries with the`LookForRegistries`
method. This functionality is *recursive*, meaning that assembly scanning declarations in the
`ServiceRegistry` types discovered through `LookForRegistries()` will also be processed.


<[sample:scan-for-registries]>


