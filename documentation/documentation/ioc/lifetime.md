<!--title:Service Lifetimes-->

Lamar's service lifetime support exactly reflects the behavior of the ASP.Net Core DI container, as [described in this article](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.0#service-lifetimes-and-registration-options). This behavior is different than the older [StructureMap lifecycle logic](http://structuremap.github.io/object-lifecycle/).

The supported lifecycles are:

1. `Singleton` -- Only one object instance is created for the entire application
1. `Scoped` -- Only one object instance is created for a container, whether that is the root container or a scoped (nested) container. This maps to StructureMap's `ContainerScoped` lifecycle
1. `Transient` -- A new object instance is created for every single request, including dependencies. This behavior is **not consistent** with StructureMap's old `Transient` and maps to StructureMap's old `AlwaysUnique` lifecycle

There is no equivalent in Lamar to StructureMap's version of `Transient` or the rarely used `ThreadLocal` lifecycle. `HttpContext` related scopes
are no longer supported, with the assumption that `Scoped` is a useful replacement for HTTP request scoping of services.

Here are some sample usages of registering services with a lifetime:

<[sample:LifetimeRegistry]>