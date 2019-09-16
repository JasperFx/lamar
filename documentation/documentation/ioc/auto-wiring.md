<!--Title: Auto Wiring-->
<!--Url: auto-wiring-->


The best way to use an IoC container is to allow "Auto Wiring" to do most of the work for you.  IoC Containers like Lamar are an infrastructure concern, and as such, should be isolated from as much of your code as possible.  Before examining Auto Wiring in depth, let's look at a common anti pattern of IoC usage:

<[sample:ShippingScreenPresenter-anti-pattern]>

Instead of binding `ShippingScreenPresenter` so tightly to Lamar and having to explicitly fetch its dependencies, let's switch
it to using Lamar a little more idiomatically and just exposing a constructor function with the necessary dependencies
as arguments:

<[sample:ShippingScreenPresenter-with-ctor-injection]>

As long as a Lamar `Container` knows how to resolve the `IRepository` and
`IShippingService` interfaces, Lamar can build `ShippingScreenPresenter` by using "auto-wiring." All this means is that
instead of forcing you to explicitly configure all the dependencies for `ShippingScreenPresenter`, Lamar can infer from
the public <[linkto:documentation/ioc/registration/constructor-selection;title=constructor function]>
what dependencies `ShippingScreenPresenter` needs and uses the defaults of both to build it out.

Looking at the <[linkto:documentation/ioc/diagnostics/build-plans;title=build plan]> for `ShippingScreenPresenter`:

<[sample:ShippingScreenPresenter-build-plan]>



