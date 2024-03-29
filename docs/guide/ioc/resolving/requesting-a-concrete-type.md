# Auto Resolving Concrete Types

Lamar allows you to resolve instances of concrete classes without configuring that concrete type with a few provisos:

* The concrete type must have at least one public constructor
* Lamar can build all the arguments in the constructor, either because Lamar has explicit configuration for that dependency or can auto resolve the type
* The constructor does not contain any _primitive_ arguments like strings, numbers, or dates because Lamar assumes those elements are configuration items and not _auto resolvable_.

Let's say we have the following object model, which represents the weather condition for a certain location.

<!-- snippet: sample_concrete-weather-model -->
<a id='snippet-sample_concrete-weather-model'></a>
```cs
public class Weather
{
    public Location Location { get; set; }
    public Atmosphere Atmosphere { get; set; }
    public Wind Wind { get; set; }
    public Condition Condition { get; set; }

    public Weather(Location location, Atmosphere atmosphere, Wind wind, Condition condition)
    {
        Location = location;
        Atmosphere = atmosphere;
        Wind = wind;
        Condition = condition;
    }
}

public class Location
{
    //some properties
}

public class Atmosphere
{
    //some properties
}

public class Wind
{
    //some properties        
}

public class Condition
{
    //some properties        
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/model.cs#L92-L129' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_concrete-weather-model' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Before we can resolve the concrete `Weather` type, we need an instance of an `Container` object. As mentioned earlier, these objects defines a generic `GetInstance` method which can build us an instance of the `Weather` type.

You can create a container yourself or use the statically accessed container.

<!-- snippet: sample_quickstart-resolve-concrete-types -->
<a id='snippet-sample_quickstart-resolve-concrete-types'></a>
```cs
var container = new Container();
var weather1 = container.GetInstance<Weather>();

var weather2 = container.GetInstance<Weather>();
weather2 = container.GetInstance<Weather>(); //short version for above.
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/quickstart/resolving_instances.cs#L44-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart-resolve-concrete-types' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The reason why we don't need to supply any configuration is because Lamar supports a concept called [auto wiring](/guide/ioc/auto-wiring). It's basically a smart way of building instances of types by looking to the constructors of the requested and all the needed underlying types. During this inspection Lamar also uses any provided configuration to help building the requested service or dependency.

In our example, where there isn't any configuration available, Lamar looks at the constructor of the requested `Weather` type. It sees that it depends on four concrete types which all have a default constructor. Lamar is therefore able to create an instance for all of them and inject them into the `Weather` constructor. After that the `Weather` instance is returned to the caller.

Most of the time you will be mapping abstractions to concrete types, but as you have seen Lamar supports other use cases as well.
