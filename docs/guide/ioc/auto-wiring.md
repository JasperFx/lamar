# Auto Wiring

The best way to use an IoC container is to allow "Auto Wiring" to do most of the work for you.  IoC Containers like Lamar are an infrastructure concern, and as such, should be isolated from as much of your code as possible.  Before examining Auto Wiring in depth, let's look at a common anti pattern of IoC usage:

<!-- snippet: sample_ShippingScreenPresenter-anti-pattern -->
<a id='snippet-sample_shippingscreenpresenter-anti-pattern'></a>
```cs
// This is the wrong way to use an IoC container.  Do NOT invoke the container from
// the constructor function.  This tightly couples the ShippingScreenPresenter to
// the IoC container in a harmful way.  This class cannot be used in either
// production or testing without a valid IoC configuration.  Plus, you're writing more
// code
public ShippingScreenPresenter(IContainer container)
{
    // It's even worse if you use a static facade to retrieve
    // a service locator!
    _service = container.GetInstance<IShippingService>();
    _repository = container.GetInstance<IRepository>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/SetterExamples.cs#L202-L215' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shippingscreenpresenter-anti-pattern' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Instead of binding `ShippingScreenPresenter` so tightly to Lamar and having to explicitly fetch its dependencies, let's switch
it to using Lamar a little more idiomatically and just exposing a constructor function with the necessary dependencies
as arguments:

<!-- snippet: sample_ShippingScreenPresenter-with-ctor-injection -->
<a id='snippet-sample_shippingscreenpresenter-with-ctor-injection'></a>
```cs
// This is the way to write a Constructor Function with an IoC tool
// Let the IoC container "inject" services from outside, and keep
// ShippingScreenPresenter ignorant of the IoC infrastructure
public ShippingScreenPresenter(IShippingService service, IRepository repository)
{
    _service = service;
    _repository = repository;
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/SetterExamples.cs#L191-L200' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shippingscreenpresenter-with-ctor-injection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As long as a Lamar `Container` knows how to resolve the `IRepository` and
`IShippingService` interfaces, Lamar can build `ShippingScreenPresenter` by using "auto-wiring." All this means is that
instead of forcing you to explicitly configure all the dependencies for `ShippingScreenPresenter`, Lamar can infer from
the public [constructor function](/guide/ioc/registration/constructor-selection)
what dependencies `ShippingScreenPresenter` needs and uses the defaults of both to build it out.

Looking at the [build plan](/guide/ioc/diagnostics/build-plans) for `ShippingScreenPresenter`:

<!-- snippet: sample_ShippingScreenPresenter-build-plan -->
<a id='snippet-sample_shippingscreenpresenter-build-plan'></a>
```cs
[Fact]
public void ShowBuildPlan()
{
    var container = new Container(_ =>
    {
        _.For<IShippingService>().Use<InternalShippingService>();
        _.For<IRepository>().Use<SimpleRepository>();
    });

    // Just proving that we can build ShippingScreenPresenter;)
    container.GetInstance<ShippingScreenPresenter>().ShouldNotBeNull();

    var buildPlan = container.Model.For<ShippingScreenPresenter>().Default.DescribeBuildPlan();

    // _output is the xUnit ITestOutputHelper here
    _output.WriteLine(buildPlan);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/SetterExamples.cs#L237-L255' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shippingscreenpresenter-build-plan' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
