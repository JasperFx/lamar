# Working with IConfiguredInstance

  The most common way for StructureMap to build or resolve a requested object is to build a concrete type directly by calling a public constructor function and optionally filling values in public setter properties. For this type of object construction, StructureMap exposes the `IConfiguredInstance` interface as a means of querying and modifying how a concrete type will be created or resolved. While the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) fluent interface provides the main way of explicitly configuring concrete type creation, the `IConfiguredInstance` interface is meant to support [conventional registration](/guide/ioc/registration/auto-registration-and-conventions), [configuration attributes](/guide/ioc/registration/attributes), and [construction policies](/guide/ioc/registration/policies).

<!-- snippet: sample_IConfiguredInstance -->
<a id='snippet-sample_iconfiguredinstance'></a>
```cs
public interface IConfiguredInstance
{
    /// <summary>
    /// The constructor function that this registration is going to use to
    /// construct the object
    /// </summary>
    ConstructorInfo Constructor { get; set; }
    
    /// <summary>
    /// The service type that you can request. This would normally be an interface or other
    /// abstraction
    /// </summary>
    Type ServiceType { get; }
    
    /// <summary>
    /// The actual, concrete type
    /// </summary>
    Type ImplementationType { get; }
    
    
    ServiceLifetime Lifetime { get; set; }
    
    /// <summary>
    /// The instance name for requesting this object by name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Inline definition of a constructor dependency.  Select the constructor argument by type and constructor name.
    ///     Use this method if there is more than one constructor arguments of the same type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="constructorArg"></param>
    /// <returns></returns>
    DependencyExpression<T> Ctor<T>(string constructorArg = null);
    
    /// <summary>
    /// Directly add or interrogate the inline dependencies for this instance
    /// </summary>
    IReadOnlyList<Instance> InlineDependencies { get; }

    /// <summary>
    /// Adds an inline dependency
    /// </summary>
    /// <param name="instance"></param>
    void AddInline(Instance instance);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/IoC/Instances/IConfiguredInstance.cs#L8-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iconfiguredinstance' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Changing the Instance Lifecycle

You can override the lifecycle of a single `IConfiguredInstance` by calling the `LifecycleIs()` methods and either supplying a type of `ILifecycle` or an `ILifecycle` object. As a quick helper, there are also extension methods for common lifecycles:

<!-- snippet: sample_iconfiguredinstance-lifecycle -->
<a id='snippet-sample_iconfiguredinstance-lifecycle'></a>
```cs
IConfiguredInstance instance
    = new ConfiguredInstance(typeof(WidgetHolder));

// Use the SingletonThing lifecycle
instance.Singleton();

// or supply an ILifecycle type
instance.SetLifecycleTo<ThreadLocalStorageLifecycle>();

// or supply an ILifecycle object
instance.SetLifecycleTo(new Lifecycles_Samples.MyCustomLifecycle());

// or override to the default "transient" lifecycle
instance.DefaultLifecycle();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/iconfigured_instance_behavior.cs#L29-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iconfiguredinstance-lifecycle' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Reflecting over Constructor Parameters

To find the constructor function parameters of an `IConfiguredInstance`, just use this syntax (it's just .Net Reflection):

<!-- snippet: sample_reflecting-over-parameters -->
<a id='snippet-sample_reflecting-over-parameters'></a>
```cs
public class GuyWithArguments
{
    public GuyWithArguments(IWidget widget, Rule rule)
    {
    }
}

[Fact]
public void reflecting_over_constructor_args()
{
    IConfiguredInstance instance = new SmartInstance<GuyWithArguments>()
        // I'm just forcing it to assign the constructor function
        .SelectConstructor(() => new GuyWithArguments(null, null));

    instance.Constructor.GetParameters().Select(x => x.Name)
        .ShouldHaveTheSameElementsAs("widget", "rule");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/iconfigured_instance_behavior.cs#L50-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_reflecting-over-parameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

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
