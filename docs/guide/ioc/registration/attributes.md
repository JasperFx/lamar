# Using Attributes for Configuration

The Lamar team believe that forcing users to spray .Net attributes all over their own code is in clear violation of our philosophy of minimal intrusion into user code. _In other words, we don't want to be MEF._

That being said, there are plenty of times when simple attribute usage is effective for one-off deviations from your normal registration conventions or cause less harm than having to constantly change a centralized `ServerRegistry` or just seem more clear and understandable to users than a convention. For those usages, Lamar 4.0 has introduced a new base class that can be extended and used to explicitly customize your Lamar configuration:

<!-- snippet: sample_LamarAttribute -->
<a id='snippet-sample_lamarattribute'></a>
```cs
/// <summary>
///     Base class for custom configuration attributes
/// </summary>
public abstract class LamarAttribute : Attribute
{
    /// <summary>
    ///     Make configuration alterations to a single IConfiguredInstance object
    /// </summary>
    /// <param name="instance"></param>
    public virtual void Alter(IConfiguredInstance instance)
    {
    }

    /// <summary>
    ///     Make configuration changes to the most generic form of Instance
    /// </summary>
    /// <param name="instance"></param>
    public virtual void Alter(Instance instance)
    {
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/LamarAttribute.cs#L6-L30' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_lamarattribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There's a couple thing to note, here about this new attribute:

* Lamar internals are looking for any attribute of the base class. Attributes that affect types are read and applied early, while attributes decorating properties or constructor parameters are only read and applied during the creation of [build plans](/guide/ioc/diagnostics/build-plans).
* Unlike many other frameworks, the attributes in Lamar are not executed at build time. Instead, Lamar uses attributes *one time* to determine the build plan.

## Attribute Targeting Service Type or Concrete Type

Take the new `[Singleton]` attribute shown below:

<!-- snippet: sample_SingletonAttribute -->
<a id='snippet-sample_singletonattribute'></a>
```cs
/// <summary>
///     Makes Lamar treat a Type as a singleton in the lifecycle scoping
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class SingletonAttribute : LamarAttribute
{
    // This method will affect single registrations
    public override void Alter(IConfiguredInstance instance)
    {
        instance.Lifetime = ServiceLifetime.Singleton;
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/SingletonAttribute.cs#L7-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_singletonattribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This new attribute can be used on either the service type (typically an interface) or on a concrete type to make an individual type registration be a singleton. You can see the usage on some types below:

<!-- snippet: sample_[Singleton]-usage -->
<a id='snippet-sample_[singleton]-usage'></a>
```cs
[Singleton]
public class SingleWidget : IWidget
{
    public void DoSomething()
    {
        throw new NotImplementedException();
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/attribute_usage.cs#L46-L57' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_[singleton]-usage' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_[singleton]-usage-1'></a>
```cs
[Singleton] // ALL Instance's of ITeamCache will be singletons by default
public interface ITeamCache { }

public class TeamCache : ITeamCache { }

public class OtherTeamCache : ITeamCache { }

public interface ITeam { }

public class Chargers : ITeam { }

[Singleton] // This specific type will be a singleton
public class Chiefs : ITeam { }
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/attribute_usage.cs#L52-L67' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_[singleton]-usage-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Built In Attributes

Lamar supplies a handful of built in attributes for customizing configuration:

* `[ValidationMethod]` - Allows you to expose [environment tests](/guide/ioc/diagnostics/environment-tests) in your Lamar registrations
* `[DefaultConstructor]` - Declare which constructor function should be used by Lamar. See [constructor selection](/guide/ioc/registration/constructor-selection) for more information
* `[Scoped]` and `[Singleton]` - These attributes, just add another mechanism for [life cycle configuration](/guide/ioc/lifetime;title=lifecycle configuration)
* `[InstanceNamed("name")]` to override the instance name of a registered concrete class
