# Bootstrapping a Container

To configure and bootstrap a Lamar container, you have a couple options. You can create a `Container` object with inline registrations:

<[sample:bootstrap-inline]>

Or pass in a configured `ServiceRegistry` object as shown below:

<[sample:bootstrap-with-registry]>

Lamar's `ServiceRegistry` supports a subset of StructureMap's old `Registry` class and should be used as a replacement when replacing StructureMap with
Lamar. We renamed the class to disambiguate the name from the many other `Registry` classes in the CLR. `ServiceRegistry` implements the [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection?view=aspnetcore-2.0) interface
from ASP.Net Core. You can also create a Lamar container by passing in an instance of `IServiceCollection` like you'd get within an ASP.Net Core application.
