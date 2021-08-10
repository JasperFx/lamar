# Auto-Registration and Conventions

Lamar has rich support for registering types by scanning assemblies and applying conventional registrations.
Between scanning and default conventions, configurations are often just a few
lines.

Also see [type scanning diagnostics](/guide/ioc/diagnostics/type-scanning) for help in understanding the assembly scanning behavior in your system.

## ServiceRegistry.Scan()

Assembly scanning operations are defined by the `ServiceRegistry.Scan()` method demonstrated below:

<!-- snippet: sample_BasicScanning -->
<a id='snippet-sample_basicscanning'></a>
```cs
public class BasicScanning : ServiceRegistry
{
    public BasicScanning()
    {
        Scan(_ =>
        {
            // Declare which assemblies to scan
            _.Assembly("Lamar.Testing");
            _.AssemblyContainingType<IWidget>();

            // Filter types
            _.Exclude(type => type.Name.Contains("Bad"));

            // A custom registration convention
            _.Convention<MySpecialRegistrationConvention>();

            // Built in registration conventions
            _.AddAllTypesOf<IWidget>().NameBy(x => x.Name.Replace("Widget", ""));
            _.WithDefaultConventions();
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/SetterExamples.cs#L135-L158' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_basicscanning' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Please note (because I've been asked this several times over the years) that each call to `ServiceRegistry.Scan()` is an entirely atomic operation that has no impact on previous or subsequent calls.

Any given call to `ServiceRegistry.Scan()` consists of three different things:

1. One or more assemblies to scan for types
1. One or more registration conventions
1. Optionally, set filters to only include certain types or exclude other types from being processed by the scanning operation

## Scan the Calling Assembly

::: tip INFO
If you are having any issues with the type scanning using the `TheCallingAssembly()` method, just replace that call with an explicit `Assembly("assembly name")` or `AssemblyContainingType<T>()` call instead of relying on Lamar to walk up the call stack to determine the entry assembly.
:::

One of the easiest ways to register types is by scanning the assembly your
registry is placed in.

**Note** if you have other registries, Lamar will not automatically
find them.

<!-- snippet: sample_scan-calling-assembly -->
<a id='snippet-sample_scan-calling-assembly'></a>
```cs
[Fact]
public void scan_but_ignore_registries_by_default()
{
    Scan(x => { x.TheCallingAssembly(); });

    TestingRegistry.WasUsed.ShouldBeFalse();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/AssemblyScannerTester.cs#L190-L199' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scan-calling-assembly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Search for Assemblies on the File System

Lamar provides facilities for registering types by finding assemblies in the application bin path:

<!-- snippet: sample_scan-filesystem -->
<a id='snippet-sample_scan-filesystem'></a>
```cs
[Fact]
public void scan_all_assemblies_in_a_folder()
{
    Scan(x => x.AssembliesFromPath(assemblyScanningFolder));
    shouldHaveFamilyWithSameName<IInterfaceInWidget5>();
    shouldHaveFamilyWithSameName<IWorker>();
    shouldNotHaveFamilyWithSameName<IDefinedInExe>();
}

[Fact]
public void scan_all_assemblies_in_application_base_directory()
{
    Scan(x => x.AssembliesFromApplicationBaseDirectory());
    shouldHaveFamilyWithSameName<IInterfaceInWidget5>();
    shouldHaveFamilyWithSameName<IWorker>();
    shouldNotHaveFamilyWithSameName<IDefinedInExe>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/AssemblyScannerTester.cs#L145-L164' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scan-filesystem' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that Lamar 4.0 does not search for `.exe` files in the assembly search. The Lamar team felt this was
problematic and "nobody would ever actually want to do that." We were wrong, and due to many user requests, you can now
**opt in** to scanning `.exe` files with a new public method on `AssemblyScanner` shown below:

<!-- snippet: sample_scan-filesystem-for-exe -->
<a id='snippet-sample_scan-filesystem-for-exe'></a>
```cs
[Fact]
public void scan_all_assemblies_in_a_folder_including_exe()
{
    Scan(x => x.AssembliesAndExecutablesFromPath(assemblyScanningFolder));

    shouldHaveFamilyWithSameName<IInterfaceInWidget5>();
    shouldHaveFamilyWithSameName<IWorker>();
    shouldHaveFamilyWithSameName<IDefinedInExe>();
}

[Fact]
public void scan_all_assemblies_in_application_base_directory_including_exe()
{
    Scan(x => x.AssembliesAndExecutablesFromApplicationBaseDirectory());

    shouldHaveFamilyWithSameName<IInterfaceInWidget5>();
    shouldHaveFamilyWithSameName<IWorker>();
    shouldHaveFamilyWithSameName<IDefinedInExe>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/AssemblyScannerTester.cs#L166-L188' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scan-filesystem-for-exe' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do be aware that while this technique is very powerful for extensibility, it's been extremely problematic for
some folks in the past. The Lamar team's recommendation for using this feature is to:

1. Make sure you have some kind of filter on the assemblies scanned for performance and predictability reasons. Either a naming convention or filter
   by an assembly attribute to narrow where Lamar looks
1. Get familiar with the new [type scanning diagnostics](/guide/ioc/diagnostics/type-scanning) introduced in 4.0;-)

Behind the scenes, Lamar is using the `Assembly.GetExportedTypes()` method from the .Net CLR to find types and this
mechanism is **very** sensitive to missing dependencies. Again, thanks to the new [type scanning diagnostics](/guide/ioc/diagnostics/type-scanning),
you now have some visibility into assembly loading failures that used to be silently swallowed internally.

## Excluding Types

Lamar also makes it easy to exclude types, either individually or by namespace.
The following examples also show how Lamar can register an assembly by providing
a type within that assembly.

Excluding additional types or namespaces is as easy as calling the corresponding method
again.

<!-- snippet: sample_scan-exclusions -->
<a id='snippet-sample_scan-exclusions'></a>
```cs
[Fact]
public void use_a_single_exclude_of_type()
{
    Scan(x =>
    {
        x.AssemblyContainingType<ITypeThatHasAttributeButIsNotInRegistry>();
        x.ExcludeType<ITypeThatHasAttributeButIsNotInRegistry>();
    });

    shouldHaveFamily<IInterfaceInWidget5>();
    shouldNotHaveFamily<ITypeThatHasAttributeButIsNotInRegistry>();
}

[Fact]
public void use_a_single_exclude2()
{
    Scan(x =>
    {
        x.AssemblyContainingType<ITypeThatHasAttributeButIsNotInRegistry>();
        x.ExcludeNamespace("StructureMap.Testing.Widget5");
    });

    shouldNotHaveFamily<IInterfaceInWidget5>();
    shouldNotHaveFamily<ITypeThatHasAttributeButIsNotInRegistry>();
}

[Fact]
public void use_a_single_exclude3()
{
    Scan(x =>
    {
        x.AssemblyContainingType<ITypeThatHasAttributeButIsNotInRegistry>();
        x.ExcludeNamespaceContainingType<ITypeThatHasAttributeButIsNotInRegistry>();
    });

    shouldNotHaveFamily<IInterfaceInWidget5>();
    shouldNotHaveFamily<ITypeThatHasAttributeButIsNotInRegistry>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/AssemblyScannerTester.cs#L285-L325' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scan-exclusions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can also ignore specific types through an attribute:

<!-- snippet: sample_using-LamarIgnore -->
<a id='snippet-sample_using-lamarignore'></a>
```cs
// This attribute causes the type scanning to ignore this type
[LamarIgnore]
public class BiHolder : IBiHolder
{
    public BiHolder(IBiGrandparent grandparent)
    {
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/do_not_blow_up_with_bi_directional_dependencies.cs#L69-L78' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-lamarignore' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Custom Registration Conventions

It's just not possible (or desirable) for Lamar to include every possible type of auto registration
convention users might want, but that's okay because Lamar allows you to create and use your own
conventions through the `IRegistrationConvention` interface:

<!-- snippet: sample_IRegistrationConvention -->
<a id='snippet-sample_iregistrationconvention'></a>
```cs
/// <summary>
///     Used to create custom type scanning conventions
/// </summary>
public interface IRegistrationConvention
{
    void ScanTypes(TypeSet types, ServiceRegistry services);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/Scanning/Conventions/IRegistrationConvention.cs#L5-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iregistrationconvention' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Let's say that you'd like a custom convention that just registers a concrete type against all the interfaces
that it implements. You could then build a custom `IRegistrationConvention` class like the following example:

<!-- snippet: sample_custom-registration-convention -->
<a id='snippet-sample_custom-registration-convention'></a>
```cs
public interface IFoo
{
}

public interface IBar
{
}

public interface IBaz
{
}

public class BusyGuy : IFoo, IBar, IBaz
{
}

// Custom IRegistrationConvention
public class AllInterfacesConvention : IRegistrationConvention
{
    public void ScanTypes(TypeSet types, ServiceRegistry services)
    {
        // Only work on concrete types
        foreach (var type in types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).Where(x => x.Name == "BusyGuy"))
        {
            // Register against all the interfaces implemented
            // by this concrete class

            foreach (var @interface in type.GetInterfaces())
            {
                services.AddTransient(@interface, type);
            }
            
        };
    }

}

[Fact]
public void use_custom_registration_convention()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            // You're probably going to want to tightly filter
            // the Type's that are applicable to avoid unwanted
            // registrations
            x.TheCallingAssembly();
            x.IncludeNamespaceContainingType<BusyGuy>();

            // Register the custom policy
            x.Convention<AllInterfacesConvention>();
        });
    });

    container.GetInstance<IFoo>().ShouldBeOfType<BusyGuy>();
    container.GetInstance<IBar>().ShouldBeOfType<BusyGuy>();
    container.GetInstance<IBaz>().ShouldBeOfType<BusyGuy>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_registration_convention.cs#L14-L76' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_custom-registration-convention' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_custom-registration-convention-1'></a>
```cs
public interface IFoo
{
}

public interface IBar
{
}

public interface IBaz
{
}

public class BusyGuy : IFoo, IBar, IBaz
{
}

// Custom IRegistrationConvention
public class AllInterfacesConvention : IRegistrationConvention
{
    public void ScanTypes(TypeSet types, Registry registry)
    {
        // Only work on concrete types
        types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).Each(type =>
        {
            // Register against all the interfaces implemented
            // by this concrete class
            type.GetInterfaces().Each(@interface => registry.For(@interface).Use(type));
        });
    }
}

[Fact]
public void use_custom_registration_convention()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            // You're probably going to want to tightly filter
            // the Type's that are applicable to avoid unwanted
            // registrations
            x.TheCallingAssembly();
            x.IncludeNamespaceContainingType<BusyGuy>();

            // Register the custom policy
            x.Convention<AllInterfacesConvention>();
        });
    });

    container.GetInstance<IFoo>().ShouldBeOfType<BusyGuy>();
    container.GetInstance<IBar>().ShouldBeOfType<BusyGuy>();
    container.GetInstance<IBaz>().ShouldBeOfType<BusyGuy>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_registration_convention.cs#L13-L68' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_custom-registration-convention-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## The Default ISomething/Something Convention

_The Lamar team contains some VB6 veterans who hate Hungarian Notation, but can't shake the "I" nomenclature._

The "default" convention simply tries to connect concrete classes to interfaces using
the I[Something]/[Something] naming convention as shown in this sample:

<!-- snippet: sample_WithDefaultConventions -->
<a id='snippet-sample_withdefaultconventions'></a>
```cs
public interface ISpaceship { }

public class Spaceship : ISpaceship { }

public interface IRocket { }

public class Rocket : IRocket { }

[Fact]
public void default_scanning_in_action()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            x.Assembly("Lamar.Testing");
            x.WithDefaultConventions();
        });
    });

    container.GetInstance<ISpaceship>().ShouldBeOfType<Spaceship>();
    container.GetInstance<IRocket>().ShouldBeOfType<Rocket>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/scanning_samples.cs#L11-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_withdefaultconventions' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_withdefaultconventions-1'></a>
```cs
public interface ISpaceship { }

public class Spaceship : ISpaceship { }

public interface IRocket { }

public class Rocket : IRocket { }

[Fact]
public void default_scanning_in_action()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            x.Assembly("StructureMap.Testing");
            x.WithDefaultConventions();
        });
    });

    container.GetInstance<ISpaceship>().ShouldBeOfType<Spaceship>();
    container.GetInstance<IRocket>().ShouldBeOfType<Rocket>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/scanning_samples.cs#L12-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_withdefaultconventions-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

By default, the behavior is to add new registrations regardless of the existing state of the existing `ServiceRegistry`. In cases where you use a mix of explicit and conventional registrations, or other cases where you use multiple `Scan()` operations, you can control the additive registration behavior as shown below:

<!-- snippet: sample_WithDefaultConventionsOptions -->
<a id='snippet-sample_withdefaultconventionsoptions'></a>
```cs
var container = new Container(_ =>
{
    _.Scan(x =>
    {
        x.Assembly("Lamar.Testing");
        
        // This is the default, add all discovered registrations
        // regardless of existing registrations
        x.WithDefaultConventions(OverwriteBehavior.Always);
        
        // Do not add any registrations if the *ServiceType*
        // is already registered. This will prevent the scanning
        // from overwriting existing default registrations
        x.WithDefaultConventions(OverwriteBehavior.Never);
        
        // Only add new ImplementationType registrations for 
        // the ServiceType. This will prevent duplicate concrete
        // types for the same ServiceType being registered by the
        // type scanning
        x.WithDefaultConventions(OverwriteBehavior.NewType);
    });
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/scanning_samples.cs#L41-L64' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_withdefaultconventionsoptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip INFO
The service lifetime override behavior was added in Lamar v4.1
:::

Lastly, you can change the default `Lifetime` of the discovered registrations like this:

<sample:WithDefaultConventionsLifetime>

Otherwise, the default registration will be `Lifetime.Transient`.

## Registering the Single Implementation of an Interface

To tell Lamar to automatically register any interface that only has one concrete implementation, use this method:

<!-- snippet: sample_SingleImplementationsOfInterface -->
<a id='snippet-sample_singleimplementationsofinterface'></a>
```cs
public interface ISong { }

public class TheOnlySong : ISong { }

[Fact]
public void only_implementation()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            x.TheCallingAssembly();
            x.SingleImplementationsOfInterface();
        });
    });

    container.GetInstance<ISong>()
        .ShouldBeOfType<TheOnlySong>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/scanning_samples.cs#L127-L148' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_singleimplementationsofinterface' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_singleimplementationsofinterface-1'></a>
```cs
public interface ISong { }

public class TheOnlySong : ISong { }

[Fact]
public void only_implementation()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            x.TheCallingAssembly();
            x.SingleImplementationsOfInterface();
        });
    });

    container.GetInstance<ISong>()
        .ShouldBeOfType<TheOnlySong>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/scanning_samples.cs#L74-L95' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_singleimplementationsofinterface-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Register all Concrete Types of an Interface

To add all concrete types that can be cast to a named plugin type, use this syntax:

<!-- snippet: sample_register-all-types-implementing -->
<a id='snippet-sample_register-all-types-implementing'></a>
```cs
public interface IFantasySeries { }

public class WheelOfTime : IFantasySeries { }

public class GameOfThrones : IFantasySeries { }

public class BlackCompany : IFantasySeries { }

[Fact]
public void register_all_types_of_an_interface()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            x.TheCallingAssembly();

            x.AddAllTypesOf<IFantasySeries>()
                .NameBy(type => type.Name.ToLower());

            // or

            x.AddAllTypesOf(typeof(IFantasySeries))
                .NameBy(type => type.Name.ToLower());
        });
    });

    container.Model.For<IFantasySeries>()
        .Instances.Select(x => x.ImplementationType)
        .OrderBy(x => x.Name)
        .ShouldHaveTheSameElementsAs(typeof(BlackCompany), typeof(GameOfThrones), typeof(WheelOfTime));

    container.GetInstance<IFantasySeries>("blackcompany").ShouldBeOfType<BlackCompany>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/scanning_samples.cs#L89-L125' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_register-all-types-implementing' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_register-all-types-implementing-1'></a>
```cs
public interface IFantasySeries { }

public class WheelOfTime : IFantasySeries { }

public class GameOfThrones : IFantasySeries { }

public class BlackCompany : IFantasySeries { }

[Fact]
public void register_all_types_of_an_interface()
{
    var container = new Container(_ =>
    {
        _.Scan(x =>
        {
            x.TheCallingAssembly();

            x.AddAllTypesOf<IFantasySeries>();

            // or

            x.AddAllTypesOf(typeof(IFantasySeries))
                .NameBy(type => type.Name.ToLower());
        });
    });

    container.Model.For<IFantasySeries>()
        .Instances.Select(x => x.ReturnedType)
        .OrderBy(x => x.Name)
        .ShouldHaveTheSameElementsAs(typeof(BlackCompany), typeof(GameOfThrones), typeof(WheelOfTime));
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/scanning_samples.cs#L39-L72' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_register-all-types-implementing-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip INFO
`T` does not have to be an interface, it's all based on the ability to cast a concrete type to `T`
:::

## Generic Types

See [generic types](/guide/ioc/generics) for an example of using the `ConnectImplementationsToTypesClosing`
mechanism for generic types.

## Register Concrete Types against the First Interface

The last built in registration convention is a mechanism to register all concrete types
that implement at least one interface against the first interface that they implement.

<!-- snippet: sample_using-RegisterConcreteTypesAgainstTheFirstInterface -->
<a id='snippet-sample_using-registerconcretetypesagainstthefirstinterface'></a>
```cs
container = new Container(x =>
{
    x.Scan(o =>
    {
        o.TheCallingAssembly();
        o.RegisterConcreteTypesAgainstTheFirstInterface();

        o.Exclude(t => t.CanBeCastTo(typeof(IGateway)));
    });
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/FirstInterfaceConventionTester.cs#L13-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-registerconcretetypesagainstthefirstinterface' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Look for Registries

::: warning
Use some caution with this feature. Many users tried to use this feature with StructureMap just to try
to break direct assembly coupling, and while it does accomplish that goal, ask yourself if the extra complexity is worth it.
:::

Lamar can automatically include other registries with the`LookForRegistries`
method. This functionality is *recursive*, meaning that assembly scanning declarations in the
`ServiceRegistry` types discovered through `LookForRegistries()` will also be processed.

<!-- snippet: sample_scan-for-registries -->
<a id='snippet-sample_scan-for-registries'></a>
```cs
[Fact]
public void Search_for_registries_when_explicitly_told()
{
    Scan(x =>
    {
        x.TheCallingAssembly();
        x.LookForRegistries();
    });

    TestingRegistry.WasUsed.ShouldBeTrue();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/AssemblyScannerTester.cs#L229-L242' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scan-for-registries' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
