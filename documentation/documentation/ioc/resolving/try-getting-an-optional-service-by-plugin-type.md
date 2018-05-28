<!--Title: Try Getting an Optional Service by Plugin Type-->
<!--Url: try-getting-an-optional-service-by-plugin-type-->


<div class="alert alert-info" role="alert">The Lamar team does not recommend using "optional" dependencies as shown in this topic, but
external frameworks like ASP.Net MVC and Web API use this concept in their IoC container integration, so here it is. The Lamar team
prefers the usage of the <a href="http://en.wikipedia.org/wiki/Null_Object_pattern">Nullo pattern</a> instead.</div>


In normal usage, if you ask Lamar for a service and Lamar doesn't recognize the requested type, the requested name, or know what the default should be for that type, Lamar will fail fast by throwing an exception rather than returning a null. Sometimes though, you may want to
retrieve an _optional_ service from Lamar that may or may not be registered in the Container. If that particular registration doesn't exist, you
just want a null value. Lamar provides first class support for _optional_ dependencies through the usage of the `IContainer.TryGetInstance()` methods.

Say you have a simple interface `IFoo` that may or may not be registered in the Container:

<[sample:optional-foo]>

In your own code you might request the `IFoo` service like the code below, knowing that you'll
take responsibility yourself for building the `IFoo` service if Lamar doesn't have a registration
for `IFoo`:

<[sample:optional-real-usage]>

Just to make this perfectly clear, if Lamar has a default registration for `IFoo`, you get this behavior:

<[sample:optional-got-it]>

If Lamar knows nothing about `IFoo`, you get a null:

<[sample:optional-dont-got-it]>


## Concrete Types

Since it's not a perfect world, there are some gotchas you need to be aware of.
While Lamar will happily _auto-resolve_ concrete types that aren't registered,
that does not apply to the `TryGetInstance` mechanism:

<[sample:optional-no-concrete]>

## Optional Generic Types

If you are using open generic types, the `TryGetInstance()` mechanism **can** close the open generic registration
to satisfy the optional dependency like this sample:

<[sample:optional-close-generics]>

