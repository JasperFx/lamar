<!--title:Lamar as IoC Container-->

Part of Lamar's mission is to be a much more performant replacement for the venerable [StructureMap](http://structuremap.github.io/) IoC container library.
As such, it supports much of the syntax of StructureMap's `IContainer` interface and [Registry DSL](http://structuremap.github.io/registration/registry-dsl/) syntax for service registrations with the hopes that Lamar can be a near drop in replacement in many systems that use StructureMap today.

Because most new server side development in .Net today is targeting ASP.Net Core, Lamar was purposely designed and built to maximize compliance
with the underlying [IoC behavior assumed by ASP.Net Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.0). To reduce friction in Lamar usage, Lamar directly implements the core ASP.Net Core abstractions for dependency injection like `IServiceProvider` and `IServiceScope`.

<[TableOfContents]>

