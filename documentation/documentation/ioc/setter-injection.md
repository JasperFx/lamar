<!--Title: Setter Injection-->
<!--Url: setter-injection-->


<[info]>
In all cases, *Setter Injection* is an opt-in feature in Lamar that has to be explicitly enabled on a case by case basis.
<[/info]>

<[warning]>
All the *Setter Injection* rules and attributes are ignored at runtime if Lamar does not know how to resolve the property type. If setter injection is not working for you, try to look at the <[linkto:documentation/ioc/diagnostics/whatdoihave]> output and <[linkto:documentation/ioc/diagnostics/type-scanning]>.
<[/warning]>

Lamar can inject dependencies into public setter properties as part of its construction process using the _Setter Injection_ form of Dependency Injection. However, the Lamar team strongly recommends using constructor injection wherever possible instead of setter injection. That being said,
there are few cases where setter injection is probably easier (inheritance hierarchies), not to mention legacy or third party tools that
simply cannot support constructor injection *cough* ASP.Net *cough*.

See this discussion from Martin Fowler on [Constructor vs Setter Injection](http://martinfowler.com/articles/injection.html#ConstructorVersusSetterInjection).

**If you are having any trouble with setter injection in your Lamar usage, make sure you're familiar with using <[linkto:documentation/ioc/diagnostics/build-plans]>
to help in troubleshooting**


## Explicit Setter Injection with [SetterProperty] Attributes

The simplest conceptual way to force Lamar into making public setters mandatory service dependencies by decorating setter properties with the `[SetterProperty]` attribute like this example:

<[sample:setter-injection-with-SetterProperty]>

Without the `[SetterProperty]` attributes decorating the setters, Lamar would ignore the `Provider` and `ShouldCache` properties when it builds up a `Repository` object. With the attributes, Lamar will try to build and attach values for the two properties as part of object construction.

If you were to look at Lamar's "build plan" for the `Repository` class, you would see the actual C# code that Lamar compiles to build the concrete objects:

<pre>
    public class Lamar_Testing_Examples_Repository_repository : Lamar.IoC.Resolvers.TransientResolver&lt;Lamar.Testing.Examples.Repository&gt;
    {

        public bool func_repository_bool {get; set;}


        public override Lamar.Testing.Examples.Repository Build(Lamar.IoC.Scope scope)
        {
            var dataProvider = new Lamar.Testing.Examples.DataProvider();
            return new Lamar.Testing.Examples.Repository(){Provider = dataProvider, ShouldCache = func_repository_bool};
        }

    }
</pre>

Alas, like almost every code generation tool in the history of computer science, the resulting code isn't terribly pretty. You may find it easier to 
read and parse by copying the code into a real class file and letting your tool of choice (ReSharper or Rider for me) reformat the code and clean up
usings.

If you intensely dislike runaway attribute usage, that's okay because there are other ways to enable setter injection in Lamar.

## Inline Setter Configuration

Any setter property not configured with `[SetterProperty]` or the setter policies in the next section can still be filled by Lamar if an inline dependency is configured matching that setter property as shown in the example below:

<[sample:inline-dependencies-setters]>

See also: <[linkto:documentation/ioc/registration/inline-dependencies]>


## Setter Injection Policies

Lastly, you can give Lamar some criteria for determining which setters should be mandatory dependencies with the `Registry.Policies.SetAllProperties()` method during Container setup as shown in this example below:

<[sample:using-setter-policy]>

All calls to `Registry.Policies.SetAllProperties()` are additive, meaning you can use as many criteria as possible for setter injection.

