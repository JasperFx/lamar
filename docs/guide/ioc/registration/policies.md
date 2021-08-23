# Policies

Lamar offers some mechanisms to conventionally determine missing registrations at runtime or apply some fine-grained control over how services are constructed. All of these mechanisms are available under the `ServiceRegistry.Policies` property when configuring a Lamar container.

For more information on type scanning conventions, see [auto-registration and conventions](/guide/ioc/registration/auto-registration-and-conventions)

## Auto Resolve Missing Services

::: tip INFO
These policies are only evaluated the first time that a particular service type is requested through the container.
:::

Lamar has a feature to create missing service registrations at runtime based on pluggable rules using the new `IFamilyPolicy`
interface:

<!-- snippet: sample_IFamilyPolicy -->
<a id='snippet-sample_ifamilypolicy'></a>
```cs
public interface IFamilyPolicy : ILamarPolicy
{
    /// <summary>
    ///     Allows you to create missing registrations for an unknown service type
    ///     at runtime.
    ///     Return null if this policy does not apply to the given type
    /// </summary>
    ServiceFamily Build(Type type, ServiceGraph serviceGraph);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/IFamilyPolicy.cs#L11-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ifamilypolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Internally, if you make a request to `IContainer.GetInstance(type)` for a type that the active `Container` does not recognize, StructureMap will next try to apply all the registered `IFamilyPolicy` policies to create a `ServiceFamily` object for that service type that models the registrations for that service type, including the default, additional named instances, interceptors or decorators, and lifecycle rules.

The simplest built in example is the `EnumerablePolicy` shown below that can fill in requests for `IList<T>`, `ICollection<T>`, and `T[]` with a collection of all the known registrations of the type `T`:

<!-- snippet: sample_EnumerablePolicy -->
<a id='snippet-sample_enumerablepolicy'></a>
```cs
internal class EnumerablePolicy : IFamilyPolicy
{
    public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
    {
        if (type.IsArray)
        {
            var instanceType = typeof(ArrayInstance<>).MakeGenericType(type.GetElementType());
            var instance = Activator.CreateInstance(instanceType, type).As<Instance>();
            return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
        }

        if (type.IsEnumerable())
        {
            var elementType = type.GetGenericArguments().First();
            
            var instanceType = typeof(ListInstance<>).MakeGenericType(elementType);
            var ctor = instanceType.GetConstructors().Single();
            var instance = ctor.Invoke(new object[]{type}).As<Instance>();
            
            return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
        }

        return null;
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/IoC/Enumerables/EnumerablePolicy.cs#L9-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enumerablepolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The result of `EnumerablePolicy` in action is shown by the acceptance test below:

<!-- snippet: sample_EnumerableFamilyPolicy_in_action -->
<a id='snippet-sample_enumerablefamilypolicy_in_action'></a>
```cs
[Fact]
public void collection_types_are_all_possible_by_default()
{
    // NOTE that we do NOT make any explicit registration of
    // IList<IWidget>, IEnumerable<IWidget>, ICollection<IWidget>, or IWidget[]
    var container = new Container(_ =>
    {
        _.For<IWidget>().Add<AWidget>();
        _.For<IWidget>().Add<BWidget>();
        _.For<IWidget>().Add<CWidget>();
    });

    // IList<T>
    container.GetInstance<IList<IWidget>>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    // ICollection<T>
    container.GetInstance<ICollection<IWidget>>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

    // Array of T
    container.GetInstance<IWidget[]>()
        .Select(x => x.GetType())
        .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/enumerable_instances.cs#L10-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_enumerablefamilypolicy_in_action' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For another example, consider this example from the Lamar unit tests. Say you have a service that looks like this:

<!-- snippet: sample_Color -->
<a id='snippet-sample_color'></a>
```cs
public class Color
{
    public string Name { get; set; }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/using_family_policies.cs#L148-L153' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_color' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And you build a policy that auto-resolves registrations for the `Color` service if none previously exist:

<!-- snippet: sample_ColorPolicy -->
<a id='snippet-sample_colorpolicy'></a>
```cs
public class ColorPolicy : IFamilyPolicy
{
    public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
    {
        if (type != typeof(Color)) return null;
        
        return new ServiceFamily(type, serviceGraph.DecoratorPolicies, 
            ObjectInstance.For(new Color{Name = "Red"}).Named("Red"),
            ObjectInstance.For(new Color{Name = "Blue"}).Named("Blue"),
            ObjectInstance.For(new Color{Name = "Green"}).Named("Green")
            
            
            );
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/using_family_policies.cs#L167-L183' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_colorpolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can register the new `ColorPolicy` shown above like this:

<!-- snippet: sample_register-ColorPolicy -->
<a id='snippet-sample_register-colorpolicy'></a>
```cs
var container = Container.For(_ =>
{
    _.Policies.OnMissingFamily<ColorPolicy>();
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/using_family_policies.cs#L132-L137' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_register-colorpolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Internally, Lamar uses this `IFamilyPolicy` feature for its [generic type support](/guide/ioc/generics), the [enumerable type support described as above](/guide/ioc/working-with-enumerable-types), and the [auto registration of concrete types](/guide/ioc/resolving/requesting-a-concrete-type).

## Instance Construction Policies

Lamar allows you to create conventional build policies with a mechanism for altering _how_ object instances are built based on user created _meta-conventions_ using the `IInstancePolicy` shown below:

<!-- snippet: sample_IInstancePolicy -->
<a id='snippet-sample_iinstancepolicy'></a>
```cs
/// <summary>
/// Custom policy on Instance construction that is evaluated
/// as part of creating a "build plan"
/// </summary>

public interface IInstancePolicy : ILamarPolicy
{
    /// <summary>
    /// Apply any conventional changes to the configuration
    /// of a single Instance
    /// </summary>
    /// <param name="instance"></param>
    void Apply(Instance instance);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/IInstancePolicy.cs#L7-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iinstancepolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

These policies are registered as part of the [ServiceRegistry DSL](/guide/ioc/registration/registry-dsl) with the `Policies.Add()` method:

<!-- snippet: sample_policies.add -->
<a id='snippet-sample_policies.add'></a>
```cs
var container = new Container(_ =>
{
    _.Policies.Add<MyCustomPolicy>();
    // or
    _.Policies.Add(new MyCustomPolicy());
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L245-L252' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_policies.add' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_policies.add-1'></a>
```cs
var container = new Container(_ =>
{
    _.Policies.Add<MyCustomPolicy>();
    // or
    _.Policies.Add(new MyCustomPolicy());
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L238-L245' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_policies.add-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `IInstancePolicy` mechanism probably works differently than other IoC containers in that the policy is applied to the container's underlying configuration model instead of at runtime. Internally, StructureMap lazily creates a ["build plan"](/guide/ioc/diagnostics/build-plans) for each configured Instance at the first time that that Instance is built or resolved. As part of creating that build plan, StructureMap runs all the registered `IInstancePolicy` objects against the Instance in question to capture any potential changes before "baking" the build plan into a .Net `Expression` that is then compiled into a `Func` for actual construction.

The `Instance` objects will give you access to the types being created, the configured name of the Instance (if any), the ability to add interceptors and to modify the lifecycle. If you wish to add inline dependencies to Instances that are built by calling constructor function and setter properties, you may find it easier to use the `ConfiguredInstancePolicy` base class as a convenience:

<!-- snippet: sample_ConfiguredInstancePolicy -->
<a id='snippet-sample_configuredinstancepolicy'></a>
```cs
/// <summary>
/// Base class for using policies against IConfiguredInstance registrations
/// </summary>
public abstract class ConfiguredInstancePolicy : IInstancePolicy
{
    public void Apply(Instance instance)
    {
        if (instance is IConfiguredInstance)
        {
            apply(instance.As<IConfiguredInstance>());
        }
    }

    protected abstract void apply(IConfiguredInstance instance);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar/IInstancePolicy.cs#L24-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuredinstancepolicy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For more information, see:

* [Build Plans](/guide/ioc/diagnostics/build-plans)
* [Working with IConfiguredInstance](/guide/ioc/registration/configured-instance)
* [Service Lifetimes](/guide/ioc/lifetime)

## Example 1: Constructor arguments

So let me say upfront that I don't like this approach, but other folks have asked for this ability over the years. Say that you have some legacy code where many concrete classes have a constructor argument called "connectionString" that needs to be the connection string to the application database like these classes:

<!-- snippet: sample_database-users -->
<a id='snippet-sample_database-users'></a>
```cs
public class DatabaseUser
{
    public string ConnectionString { get; set; }

    public DatabaseUser(string connectionString)
    {
        ConnectionString = connectionString;
    }
}

public class ConnectedThing
{
    public string ConnectionString { get; set; }

    public ConnectedThing(string connectionString)
    {
        ConnectionString = connectionString;
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L14-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_database-users' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_database-users-1'></a>
```cs
public class DatabaseUser
{
    public string ConnectionString { get; set; }

    public DatabaseUser(string connectionString)
    {
        ConnectionString = connectionString;
    }
}

public class ConnectedThing
{
    public string ConnectionString { get; set; }

    public ConnectedThing(string connectionString)
    {
        ConnectionString = connectionString;
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L11-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_database-users-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Instead of explicitly configuring every single concrete class in StructureMap with that inline constructor argument, we can make a policy to do that in one place:

<!-- snippet: sample_connectionstringpolicy -->
<a id='snippet-sample_connectionstringpolicy'></a>
```cs
public class ConnectionStringPolicy : ConfiguredInstancePolicy
{
    protected override void apply(IConfiguredInstance instance)
    {
        var parameter = instance.ImplementationType
            .GetConstructors()
            .SelectMany(x => x.GetParameters())
            .FirstOrDefault(x => x.Name == "connectionString");
        
        if (parameter != null)
        {
            var connectionString = findConnectionStringFromConfiguration();
            instance.Ctor<string>(parameter.Name).Is(connectionString);
        }
    }

    // find the connection string from whatever configuration
    // strategy your application uses
    private string findConnectionStringFromConfiguration()
    {
        return "the connection string";
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L37-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_connectionstringpolicy' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_connectionstringpolicy-1'></a>
```cs
public class ConnectionStringPolicy : ConfiguredInstancePolicy
{
    protected override void apply(Type pluginType, IConfiguredInstance instance)
    {
        var parameter = instance.Constructor.GetParameters().FirstOrDefault(x => x.Name == "connectionString");
        if (parameter != null)
        {
            var connectionString = findConnectionStringFromConfiguration();
            instance.Dependencies.AddForConstructorParameter(parameter, connectionString);
        }
    }

    // find the connection string from whatever configuration
    // strategy your application uses
    private string findConnectionStringFromConfiguration()
    {
        return "the connection string";
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L34-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_connectionstringpolicy-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, let's use that policy against the types that need "connectionString" and see what happens:

<!-- snippet: sample_use_the_connection_string_policy -->
<a id='snippet-sample_use_the_connection_string_policy'></a>
```cs
[Fact]
public void use_the_connection_string_policy()
{
    var container = new Container(_ =>
    {
        _.Policies.Add<ConnectionStringPolicy>();
    });

    container.GetInstance<DatabaseUser>()
        .ConnectionString.ShouldBe("the connection string");

    container.GetInstance<ConnectedThing>()
        .ConnectionString.ShouldBe("the connection string");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L64-L80' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_use_the_connection_string_policy' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_use_the_connection_string_policy-1'></a>
```cs
[Fact]
public void use_the_connection_string_policy()
{
    var container = new Container(_ =>
    {
        _.Policies.Add<ConnectionStringPolicy>();
    });

    container.GetInstance<DatabaseUser>()
        .ConnectionString.ShouldBe("the connection string");

    container.GetInstance<ConnectedThing>()
        .ConnectionString.ShouldBe("the connection string");
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L57-L73' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_use_the_connection_string_policy-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Years ago StructureMap was knocked by an "IoC expert" for not having this functionality. I said at the time -- and still would -- that I would strongly recommend that you simply **don't directly
open database connections** in more than one or a very few spots in your code anyway. If I did need to configure a database connection string in multiple concrete classes, I prefer [strong typed configuration](http://jeremydmiller.com/2014/11/07/strong_typed_configuration/).

## Example 2: Connecting to Databases based on Parameter Name

From another common user request over the years, let's say that your application needs to connect to multiple databases, but your data access service in both cases is an interface called `IDatabase`, and that's all the consumers of any database should ever need to know.

To make this concrete, let's say that our data access is all behind an interface and concrete class pair named `Database/IDatabase` like so:

<!-- snippet: sample_IDatabase -->
<a id='snippet-sample_idatabase'></a>
```cs
public interface IDatabase { }

public class Database : IDatabase
{
    public string ConnectionString { get; set; }

    public Database(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public override string ToString()
    {
        return string.Format("ConnectionString: {0}", ConnectionString);
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L82-L100' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_idatabase' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_idatabase-1'></a>
```cs
public interface IDatabase { }

public class Database : IDatabase
{
    public string ConnectionString { get; set; }

    public Database(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public override string ToString()
    {
        return string.Format("ConnectionString: {0}", ConnectionString);
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L75-L93' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_idatabase-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For a registration policy, let's say that the parameter name of an `IDatabase` dependency in a constructor function should match an identifier of one of the registered `IDatabase` services.

That policy would be:

<!-- snippet: sample_InjectDatabaseByName -->
<a id='snippet-sample_injectdatabasebyname'></a>
```cs
public class InjectDatabaseByName : ConfiguredInstancePolicy
{
    protected override void apply(IConfiguredInstance instance)
    {
        instance.ImplementationType.GetConstructors()
            .SelectMany(x => x.GetParameters())
            .Where(x => x.ParameterType == typeof(IDatabase))
            .Each(param =>
            {
                // Using ReferencedInstance here tells Lamar
                // to "use the IDatabase by this name"
                instance.Ctor<IDatabase>(param.Name).IsNamedInstance(param.Name);
            });
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L141-L158' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_injectdatabasebyname' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_injectdatabasebyname-1'></a>
```cs
public class InjectDatabaseByName : ConfiguredInstancePolicy
{
    protected override void apply(Type pluginType, IConfiguredInstance instance)
    {
        instance.Constructor.GetParameters()
            .Where(x => x.ParameterType == typeof(IDatabase))
            .Each(param =>
            {
                // Using ReferencedInstance here tells StructureMap
                // to "use the IDatabase by this name"
                var db = new ReferencedInstance(param.Name);
                instance.Dependencies.AddForConstructorParameter(param, db);
            });
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L134-L151' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_injectdatabasebyname-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And because I'm generally pretty boring about picking test data names, let's say that two of
our databases are named "red" and "green" with this container registration below:

<!-- snippet: sample_choose_database_container_setup -->
<a id='snippet-sample_choose_database_container_setup'></a>
```cs
var container = new Container(_ =>
{
    _.For<IDatabase>().Add<Database>().Named("red")
        .Ctor<string>("connectionString").Is("*red*");

    _.For<IDatabase>().Add<Database>().Named("green")
        .Ctor<string>("connectionString").Is("*green*");

    _.Policies.Add<InjectDatabaseByName>();
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L163-L174' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_choose_database_container_setup' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_choose_database_container_setup-1'></a>
```cs
var container = new Container(_ =>
{
    _.For<IDatabase>().Add<Database>().Named("red")
        .Ctor<string>("connectionString").Is("*red*");

    _.For<IDatabase>().Add<Database>().Named("green")
        .Ctor<string>("connectionString").Is("*green*");

    _.Policies.Add<InjectDatabaseByName>();
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L156-L167' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_choose_database_container_setup-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For more context, the classes that use `IDatabase` would need to have constructor functions like
these below:

<!-- snippet: sample_database-users-2 -->
<a id='snippet-sample_database-users-2'></a>
```cs
public class BigService
{
    public BigService(IDatabase green)
    {
        DB = green;
    }

    public IDatabase DB { get; set; }
}

public class ImportantService
{
    public ImportantService(IDatabase red)
    {
        DB = red;
    }

    public IDatabase DB { get; set; }
}

public class DoubleDatabaseUser
{
    public DoubleDatabaseUser(IDatabase red, IDatabase green)
    {
        Red = red;
        Green = green;
    }

    // Watch out for potential conflicts between setters
    // and ctor params. The easiest thing is to just make
    // setters private
    public IDatabase Green { get; private set; }

    public IDatabase Red { get; private set; }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L102-L139' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_database-users-2' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_database-users-2-1'></a>
```cs
public class BigService
{
    public BigService(IDatabase green)
    {
        DB = green;
    }

    public IDatabase DB { get; set; }
}

public class ImportantService
{
    public ImportantService(IDatabase red)
    {
        DB = red;
    }

    public IDatabase DB { get; set; }
}

public class DoubleDatabaseUser
{
    public DoubleDatabaseUser(IDatabase red, IDatabase green)
    {
        Red = red;
        Green = green;
    }

    // Watch out for potential conflicts between setters
    // and ctor params. The easiest thing is to just make
    // setters private
    public IDatabase Green { get; private set; }

    public IDatabase Red { get; private set; }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L95-L132' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_database-users-2-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Finally, we can exercise our new policy and see it in action:

<!-- snippet: sample_inject-database-by-name-in-usage -->
<a id='snippet-sample_inject-database-by-name-in-usage'></a>
```cs
// ImportantService should get the "red" database
container.GetInstance<ImportantService>()
    .DB.As<Database>().ConnectionString.ShouldBe("*red*");

// BigService should get the "green" database
container.GetInstance<BigService>()
    .DB.As<Database>().ConnectionString.ShouldBe("*green*");

// DoubleDatabaseUser gets both
var user = container.GetInstance<DoubleDatabaseUser>();

user.Green.As<Database>().ConnectionString.ShouldBe("*green*");
user.Red.As<Database>().ConnectionString.ShouldBe("*red*");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L176-L190' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inject-database-by-name-in-usage' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_inject-database-by-name-in-usage-1'></a>
```cs
// ImportantService should get the "red" database
container.GetInstance<ImportantService>()
    .DB.As<Database>().ConnectionString.ShouldBe("*red*");

// BigService should get the "green" database
container.GetInstance<BigService>()
    .DB.As<Database>().ConnectionString.ShouldBe("*green*");

// DoubleDatabaseUser gets both
var user = container.GetInstance<DoubleDatabaseUser>();

user.Green.As<Database>().ConnectionString.ShouldBe("*green*");
user.Red.As<Database>().ConnectionString.ShouldBe("*red*");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L169-L183' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_inject-database-by-name-in-usage-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**How I prefer to do this** - my strong preference would be to use separate interfaces for the different
databases even if that type is just an empty type marker that implements the same base.
I feel like using separate interfaces makes the code easier to trace and understand than trying
to make StructureMap vary dependencies based on naming conventions or what namespace a concrete type
happens to be in. At least now though, you have the choice of my way or using policies based on
naming conventions.

## Example 3: Make objects singletons based on type name

Unlike the top two examples, this is taken from a strategy that I used in [FubuMVC](http://github.com/darthfubumvc/fubumvc)
for its service registration. In that case, we wanted any concrete type whose name ended with
"Cache" to be a singleton in the container registration. With the new `IInstancePolicy` feature in StructureMap 4,
we could create a new policy class like so:

<!-- snippet: sample_CacheIsSingleton -->
<a id='snippet-sample_cacheissingleton'></a>
```cs
public class CacheIsSingleton : IInstancePolicy
{
    public void Apply(Instance instance)
    {
        if (instance.ImplementationType.Name.EndsWith("Cache"))
        {
            instance.Lifetime = ServiceLifetime.Singleton;
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L197-L209' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_cacheissingleton' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_cacheissingleton-1'></a>
```cs
public class CacheIsSingleton : IInstancePolicy
{
    public void Apply(Type pluginType, Instance instance)
    {
        if (instance.ReturnedType.Name.EndsWith("Cache"))
        {
            instance.SetLifecycleTo<SingletonLifecycle>();
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L190-L202' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_cacheissingleton-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Now, let's say that we have an interface named `IWidgets` and a single implementation called `WidgetCache` that
should track our widgets in the application. Using our new policy, we should see `WidgetCache` being
made a singleton:

<!-- snippet: sample_set_cache_to_singleton -->
<a id='snippet-sample_set_cache_to_singleton'></a>
```cs
[Fact]
public void set_cache_to_singleton()
{
    var container = new Container(_ =>
    {
        _.Policies.Add<CacheIsSingleton>();

        _.For<IWidgets>().Use<WidgetCache>();
    });

    // The policy is applied *only* at the time
    // that StructureMap creates a "build plan"
    container.GetInstance<IWidgets>()
        .ShouldBeSameAs(container.GetInstance<IWidgets>());

    // Now that the policy has executed, we
    // can verify that WidgetCache is a SingletonThing
    container.Model.For<IWidgets>().Default
            .Lifetime.ShouldBe(ServiceLifetime.Singleton);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/custom_policies.cs#L211-L233' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_set_cache_to_singleton' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_set_cache_to_singleton-1'></a>
```cs
[Fact]
public void set_cache_to_singleton()
{
    var container = new Container(_ =>
    {
        _.Policies.Add<CacheIsSingleton>();

        _.For<IWidgets>().Use<WidgetCache>();
    });

    // The policy is applied *only* at the time
    // that StructureMap creates a "build plan"
    container.GetInstance<IWidgets>()
        .ShouldBeTheSameAs(container.GetInstance<IWidgets>());

    // Now that the policy has executed, we
    // can verify that WidgetCache is a SingletonThing
    container.Model.For<IWidgets>().Default
        .Lifecycle.ShouldBeOfType<SingletonLifecycle>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Acceptance/custom_policies.cs#L204-L226' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_set_cache_to_singleton-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
