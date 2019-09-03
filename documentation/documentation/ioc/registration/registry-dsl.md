<!--Title: ServiceRegistry DSL-->

<[info]>
Lamar's *ServiceRegistry* is the equivalent to StructureMap's *Registry*, but the name was changed
to disambiguate from the nearly infinite number of other *Registry* types in .Net.
<[/info]>

Creating `ServiceRegistry` classes is the recommended way of using the Registry DSL. 

The Registry DSL is mostly a [fluent interface][1] with some nested [closure][2] 
usage. The intent of the Registry DSL is to make the configuration process as 
error free as possible by using "compiler safe" expressions and defensive 
programming to point out missing data.

## The ServiceRegistry Class

On all but the smallest systems, the main unit of configuration will probably be 
the `ServiceRegistry` class.  Typically, you would subclass the `ServiceRegistry` class, then 
use the [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface) methods exposed by the Registry class to create Container 
configuration. Here's a sample `ServiceRegistry` class below used to configure an 
instance of an `IWidget` interface:

<[sample:simple-registry]>

## Including Other ServiceRegistry Objects

The next question is "how does my new `ServiceRegistry` class get used?" 

When you set up a `Container`, you need to simply direct the 
`Container` to use the configuration in that `ServiceRegistry` class:

<[sample:including-registries]>


## _Use_ versus _Add_

<[warning]>
This behavior changed from StructureMap. Lamar follows the now common approach mandated by ASP.Net Core that the last registration
for a certain service type wins. So no more special meaning to Use() vs. Add().
<[/warning]>

**There is no difference in behavior between *Use* and *Add* in Lamar**. The two methods are synonyms and
mostly remain in Lamar to provide and easier migration path from [StructureMap](https://structuremap.github.io). The 



## Registrations with For().Use()/Add()

To register an `Instance` of a type, the syntax is one of the `Registry.For().Use()` overloads shown below:

<[sample:SettingDefaults]>

or

<[sample:AdditionalRegistrations]>



## Add Many Registrations with For().AddInstances()

If you need to add several `Instances` to a single plugin type, the `AddInstances()` syntax
shown below may be quicker and easier to use:

<[sample:Using-AddInstances]>


## Named Instances

When you have multiple implementations of an interface, it can often be useful to
name instances. To retrieve a specific implementation:

<[sample:named-instance]>

You can also register named instances with the following shorthand:

<[sample:named-instances-shorthand]>

## Inverse Registrations with Use().For()

In some scenarios, a type may implement multiple interfaces.
You could register this with a separate `For().Use()` line for each interface, but if
the type is to be a singleton, then registering it this way will give you a 
*different* singleton instance for each interface. To use the same instance across multiple
interfaces, you can use the reverse syntax.

<[sample:inverse-registration]>

The same thing works for scoped registrations; using `.Scoped()` in place of `.Singleton()` in
the above sample would result in the same instance being returned when resolving any one of the 
registered interfaces for the duration of the scope.

A transient registration can also be made using `.Transient()`, in which case the behaviour is exactly
the same as with the more usual `For().Use()` syntax; it's just a convenient shorthand in the
case of a type that implements many interfaces.

[1]: http://martinfowler.com/bliki/FluentInterface.html
[2]: http://en.wikipedia.org/wiki/Closure_%28computer_programming%29

