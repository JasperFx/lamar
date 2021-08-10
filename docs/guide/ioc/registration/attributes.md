# Using Attributes for Configuration

The Lamar team believe that forcing users to spray .Net attributes all over their own code is in clear violation of our philosophy of minimal intrusion into user code. _In other words, we don't want to be MEF._

That being said, there are plenty of times when simple attribute usage is effective for one-off deviations from your normal registration conventions or cause less harm than having to constantly change a centralized `ServerRegistry` or just seem more clear and understandable to users than a convention. For those usages, Lamar 4.0 has introduced a new base class that can be extended and used to explicitly customize your Lamar configuration:

<[sample:LamarAttribute]>

There's a couple thing to note, here about this new attribute:

* Lamar internals are looking for any attribute of the base class. Attributes that affect types are read and applied early, while attributes decorating properties or constructor parameters are only read and applied during the creation of [build plans](/guide/ioc/diagnostics/build-plans).
* Unlike many other frameworks, the attributes in Lamar are not executed at build time. Instead, Lamar uses attributes *one time* to determine the build plan.

## Attribute Targeting Plugin Type or Concrete Type

Take the new `[Singleton]` attribute shown below:

<[sample:SingletonAttribute]>

This new attribute can be used on either the plugin type (typically an interface) or on a concrete type to make an individual type registration be a singleton. You can see the usage on some types below:

<[sample:[Singleton]-usage]>

## Built In Attributes

Lamar supplies a handful of built in attributes for customizing configuration:

* `[ValidationMethod]` - Allows you to expose [environment tests](/guide/ioc/diagnostics/environment-tests) in your Lamar registrations
* `[DefaultConstructor]` - Declare which constructor function should be used by Lamar. See [constructor selection](/guide/ioc/registration/constructor-selection) for more information
* `[Scoped]` and `[Singleton]` - These attributes, just add another mechanism for [life cycle configuration](/guide/ioc/lifetime;title=lifecycle configuration)
* `[InstanceNamed("name")]` to override the instance name of a registered concrete class
