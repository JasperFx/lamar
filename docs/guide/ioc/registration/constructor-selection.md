# Constructor Selection

Lamar's constructor selection logic is compliant with the ASP.Net Core DI specifications and uses their definition of selecting the "greediest constructor." Definitely note that **this behavior is different than StructureMap's version of "greediest constructor" selection**.

Constructor selection can be happily overridden by using one of the mechanisms shown below or using custom [instance policies](/guide/ioc/registration/policies;title=instance policies).

## Greediest Constructor

If there are multiple public constructor functions on a concrete class, Lamar's default behavior is to select the "greediest" constructor where Lamar can resolve all of the parameters, i.e., the constructor function with the most parameters. In the case of two or more constructor functions with the same number of parameters Lamar will simply take the first constructor encountered in that subset of constructors assuming all the constructor parameter lists can be filled by the container.

The default constructor selection is demonstrated below:

<[sample:select-the-greediest-ctor]>

The "greediest constructor selection" will bypass any constructor function that requires "simple" arguments like strings, numbers, or enumeration values that are not explicitly configured for the instance.

You can see this behavior shown below:

<[sample:skip-ctor-with-missing-simples]>

## Explicitly Selecting a Constructor

To override the constructor selection explicitly on a case by case basis, you
can use the `SelectConstructor(Expression)` method in the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) as shown below:

<[sample:explicit-ctor-selection]>

## [DefaultConstructor] Attribute

Alternatively, you can override the choice of constructor function by using the older `[DefaultConstructor]` attribute like this:

<[sample:using-default-ctor-attribute]>
