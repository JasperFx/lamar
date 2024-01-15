# ServiceRegistry DSL

::: tip INFO
Lamar's *ServiceRegistry* is the equivalent to StructureMap's *Registry*, but the name was changed
to disambiguate from the nearly infinite number of other *Registry* types in .NET.
:::

Creating `ServiceRegistry` classes is the recommended way of using the Registry DSL.

The Registry DSL is mostly a [fluent interface][1] with some nested [closure][2] usage. The intent of the Registry DSL is to make the configuration process as error free as possible by using "compiler safe" expressions and defensive programming to point out missing data.

## The ServiceRegistry Class

On all but the smallest systems, the main unit of configuration will probably be the `ServiceRegistry` class.  Typically, you would subclass the `ServiceRegistry` class, then use the [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface) methods exposed by the Registry class to create Container configuration. Here's a sample `ServiceRegistry` class below used to configure an instance of an `IWidget` interface:

<!-- snippet: sample_simple-registry -->
<a id='snippet-sample_simple-registry'></a>
```cs
public class PurpleRegistry : ServiceRegistry
{
    public PurpleRegistry()
    {
        For<IWidget>().Use<AWidget>();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Configuration/DSL/RegistryTester.cs#L35-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_simple-registry' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Including Other ServiceRegistry Objects

The next question is "how does my new `ServiceRegistry` class get used?"

When you set up a `Container`, you need to simply direct the `Container` to use the configuration in that `ServiceRegistry` class:

<!-- snippet: sample_including-registries -->
<a id='snippet-sample_including-registries'></a>
```cs
[Fact]
public void include_a_registry()
{
    var registry = new Registry();
    registry.IncludeRegistry<YellowBlueRegistry>();
    registry.IncludeRegistry<RedGreenRegistry>();
    registry.IncludeRegistry<PurpleRegistry>();
    // build a container
    var container = new Container(registry);
    // verify the default implementation and total registered implementations
    container.GetInstance<IWidget>().ShouldBeOfType<AWidget>();
    container.GetAllInstances<IWidget>().Count().ShouldBe(5);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Configuration/DSL/RegistryTester.cs#L101-L116' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_including-registries' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## _Use_ versus _Add_

::: warning
This behavior changed from StructureMap. Lamar follows the now common approach mandated by ASP.Net Core that the last registration
for a certain service type wins. So no more special meaning to Use() vs. Add().
:::

**There is no difference in behavior between *Use* and *Add* in Lamar**. The two methods are synonyms and
mostly remain in Lamar to provide and easier migration path from [StructureMap](https://structuremap.github.io).

## Registrations with For().Use()/Add()

To register an `Instance` of a type, the syntax is one of the `Registry.For().Use()` overloads shown below:

<!-- snippet: sample_SettingDefaults -->
<a id='snippet-sample_settingdefaults'></a>
```cs
public class SettingDefaults : ServiceRegistry
{
    public SettingDefaults()
    {
        // If you know the plugin type and its a closed type
        // you can use this syntax
        For<IWidget>().Use<DefaultWidget>();

        // By Lambda
        For<IWidget>().Use(() => new DefaultWidget());

        // Pre-existing object
        For<IWidget>().Use(new AWidget());

        // This is rare now, but still valid
        For<IWidget>().Add<AWidget>().Named("A");
        For<IWidget>().Add<BWidget>().Named("B");
        For<IWidget>().Use("A"); // makes AWidget the default

        // Also rare, but you can supply an Instance object
        // yourself for special needs
        For<IWidget>().UseInstance(new MySpecialInstance());

        // If you're registering an open generic type
        // or you just have Type objects, use this syntax
        For(typeof (IService<>)).Use(typeof (Service<>));

        // This is occasionally useful for generic types
        For(typeof (IService<>)).Use(new MySpecialInstance());
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/basic_registrations.cs#L22-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_settingdefaults' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

or

<!-- snippet: sample_AdditionalRegistrations -->
<a id='snippet-sample_additionalregistrations'></a>
```cs
public class AdditionalRegistrations : ServiceRegistry
{
    public AdditionalRegistrations()
    {
        // If you know the plugin type and its a closed type
        // you can use this syntax
        For<IWidget>().Add<DefaultWidget>();

        // By Lambda
        For<IWidget>().Add(() => new DefaultWidget());

        // Pre-existing object
        For<IWidget>().Add(new AWidget());

        // Also rare, but you can supply an Instance object
        // yourself for special needs
        For<IWidget>().AddInstance(new MySpecialInstance());

        // If you're registering an open generic type
        // or you just have Type objects, use this syntax
        For(typeof(IService<>)).Add(typeof(Service<>));

        // This is occasionally useful for generic types
        For(typeof(IService<>)).Add(new MySpecialInstance());
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/basic_registrations.cs#L55-L82' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_additionalregistrations' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Add Many Registrations with For().AddInstances()

If you need to add several `Instances` to a single service type, the `AddInstances()` syntax
shown below may be quicker and easier to use:

<!-- snippet: sample_Using-AddInstances -->
<a id='snippet-sample_using-addinstances'></a>
```cs
// registry is a StructureMap Registry object
registry.For<IService>().AddInstances(x =>
{
    // Equivalent to For<IService>().Add<ColorService>().....
    x.Type<ColorService>().Named("Red").Ctor<string>("color").Is("Red");

    // Equivalent to For<IService>().Add(new ColorService("Yellow"))......
    x.Object(new ColorService("Yellow")).Named("Yellow");

    // Equivalent to For<IService>().Use(() => new ColorService("Purple"))....
    x.ConstructedBy(() => new ColorService("Purple")).Named("Purple");

    x.Type<ColorService>().Named("Decorated").Ctor<string>("color").Is("Orange");
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Configuration/DSL/InterceptAllInstancesOfPluginTypeTester.cs#L31-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-addinstances' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Named Instances

When you have multiple implementations of an interface, it can often be useful to
name instances. To retrieve a specific implementation:

<!-- snippet: sample_named-instance -->
<a id='snippet-sample_named-instance'></a>
```cs
[Fact]
public void SimpleCaseWithNamedInstance()
{
    container = new Container(x => { x.For<IWidget>().Add<AWidget>().Named("MyInstance"); });
    // retrieve an instance by name
    var widget = (AWidget)container.GetInstance<IWidget>("MyInstance");
    widget.ShouldNotBeNull();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Configuration/DSL/AddInstanceTester.cs#L63-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_named-instance' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can also register named instances with the following shorthand:

<!-- snippet: sample_named-instances-shorthand -->
<a id='snippet-sample_named-instances-shorthand'></a>
```cs
[Fact]
public void A_concrete_type_is_available_by_name_when_it_is_added_by_the_shorthand_mechanism()
{
    IContainer container = new Container(r => r.For<IAddTypes>().AddInstances(x =>
    {
        x.Type<RedAddTypes>().Named("Red");
        x.Type<GreenAddTypes>().Named("Green");
        x.Type<BlueAddTypes>().Named("Blue");
        x.Type<PurpleAddTypes>();
    }));
    // retrieve the instances by name
    container.GetInstance<IAddTypes>("Red").IsType<RedAddTypes>();
    container.GetInstance<IAddTypes>("Green").IsType<GreenAddTypes>();
    container.GetInstance<IAddTypes>("Blue").IsType<BlueAddTypes>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Configuration/DSL/AddTypesTester.cs#L29-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_named-instances-shorthand' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Inverse Registrations with Use().For()

In some scenarios, a type may implement multiple interfaces.
You could register this with a separate `For().Use()` line for each interface, but if the type is to be a singleton, then registering it this way will give you a *different* singleton instance for each interface. To use the same instance across multiple interfaces, you can use the reverse syntax.

<!-- snippet: sample_inverse-registration -->
<a id='snippet-sample_inverse-registration'></a>
```cs
[Fact]
public void when_singleton_both_interfaces_give_same_instance()
{
    var container = new Container(services =>
    {
        services.Use<Implementation>()
            .Singleton()
            .For<IServiceA>()
            .For<IServiceB>();
    });

    var instanceA = container.GetInstance<IServiceA>();
    var instanceB = container.GetInstance<IServiceB>();

    instanceA.ShouldBeTheSameAs(instanceB);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/one_instance_across_multiple_interfaces.cs#L7-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inverse-registration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The same thing works for scoped registrations; using `.Scoped()` in place of `.Singleton()` in the above sample would result in the same instance being returned when resolving any one of the registered interfaces for the duration of the scope.

A transient registration can also be made using `.Transient()`, in which case the behaviour is exactly the same as with the more usual `For().Use()` syntax; it's just a convenient shorthand in the case of a type that implements many interfaces.

[1]: http://martinfowler.com/bliki/FluentInterface.html
[2]: http://en.wikipedia.org/wiki/Closure_%28computer_programming%29
