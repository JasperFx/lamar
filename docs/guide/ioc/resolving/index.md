# Resolving Services

This will be the easy part of interacting with Lamar. During application execution, you will need to _resolve_ the services you previously registered in the container. When you ask Lamar to resolve a service, Lamar either creates a new object instance or finds the previously built object for the correct [service lifetimes](/guide/ioc/lifetime).

While in many systems you will probably only resolve the default service of a type or a named instance of a service, there are far more ways to resolve services exposed by StructureMap. The `IContainer` interface acts as a [Service Locator](http://en.wikipedia.org/wiki/Service_locator_pattern) to build and resolve configured services on demand.
