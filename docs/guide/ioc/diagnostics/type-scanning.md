# Type Scanning Diagnostics

Type scanning and conventional auto-registration is a very powerful feature in Lamar, but it has been frequently troublesome to users when things go wrong. To try to alleviate problems, Lamar has some functionality for detecting and diagnosing problems with type scanning, mostly related to Assembly's being missing.

## Assert for Assembly Loading Failures

At its root, most type scanning and auto-registration schemes in .Net frameworks rely on the [Assembly.GetExportedTypes()](https://msdn.microsoft.com/en-us/library/system.reflection.assembly.getexportedtypes%28v=vs.110%29.aspx) method. Unfortunately, that method can be brittle and fail whenever any dependency of that Assembly cannot be loaded into the current process, even if your application has no need for that dependency. In Lamar, you can use this method to assert the presence of any assembly load exceptions during type scanning:

<!-- snippet: sample_assert-no-type-scanning-failures -->
<a id='snippet-sample_assert-no-type-scanning-failures'></a>
```cs
TypeRepository.AssertNoTypeScanningFailures();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/Scanning/TypeRepositoryTester.cs#L45-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_assert-no-type-scanning-failures' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The method above will throw an exception listing all the Assembly's that failed during the call to `GetExportedTypes()` only if there were any failures. Use this method during your application bootstrapping if you want it to fail fast with any type scanning problems.

## What did Lamar scan?

Confusion of type scanning has been a constant problem with Lamar usage over the years -- especially if users are trying to dynamically load assemblies from the file system for extensibility. In order to see into what Lamar has done with type scanning, 4.0 introduces the `Container.WhatDidIScan()` method.

Let's say that you have a `Container` that is set up with at least two different scanning operations like this sample from the Lamar unit tests:

<!-- snippet: sample_whatdidiscan -->
<a id='snippet-sample_whatdidiscan'></a>
```cs
var container = new Container(_ =>
{
    _.Scan(x =>
    {
        x.TheCallingAssembly();

        x.WithDefaultConventions();
        x.RegisterConcreteTypesAgainstTheFirstInterface();
        x.SingleImplementationsOfInterface();
    });

    _.Scan(x =>
    {
        // Give your scanning operation a descriptive name
        // to help the diagnostics to be more useful
        x.Description = "Second Scanner";

        x.AssembliesFromApplicationBaseDirectory(assem => assem.FullName.Contains("Widget"));
        x.ConnectImplementationsToTypesClosing(typeof(IService<>));
        x.AddAllTypesOf<IWidget>();
    });
});

Console.WriteLine(container.WhatDidIScan());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDidIScan_smoke_tests.cs#L16-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdidiscan' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_whatdidiscan-1'></a>
```cs
var container = new Container(_ =>
{
    _.Scan(x =>
    {
        x.TheCallingAssembly();

        x.WithDefaultConventions();
        x.RegisterConcreteTypesAgainstTheFirstInterface();
        x.SingleImplementationsOfInterface();
    });

    _.Scan(x =>
    {
        // Give your scanning operation a descriptive name
        // to help the diagnostics to be more useful
        x.Description = "Second Scanner";

        x.AssembliesFromApplicationBaseDirectory(assem => assem.FullName.Contains("Widget"));
        x.ConnectImplementationsToTypesClosing(typeof(IService<>));
        x.AddAllTypesOf<IWidget>();
    });
});

Debug.WriteLine(container.WhatDidIScan());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDidIScan_smoke_tester.cs#L14-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdidiscan-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The resulting textual report is shown below:

_Sorry for the formatting and color of the text, but the markdown engine does **not** like the textual report_
<!-- snippet: sample_whatdidiscan-result -->
<a id='snippet-sample_whatdidiscan-result'></a>
```cs
/*
All Scanners
================================================================
Scanner #1
Assemblies
----------
* StructureMap.Testing, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Default I[Name]/[Name] registration convention
* Register all concrete types against the first interface (if any) that they implement
* Register any single implementation of any interface against that interface

Second Scanner
Assemblies
----------
* StructureMap.Testing.GenericWidgets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget2, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget3, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget4, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget5, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Connect all implementations of open generic type IService<T>
* Find and register all types implementing StructureMap.Testing.Widget.IWidget

*/
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDidIScan_smoke_tests.cs#L87-L120' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdidiscan-result' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_whatdidiscan-result-1'></a>
```cs
/*
All Scanners
================================================================
Scanner #1
Assemblies
----------
* StructureMap.Testing, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Default I[Name]/[Name] registration convention
* Register all concrete types against the first interface (if any) that they implement
* Register any single implementation of any interface against that interface

Second Scanner
Assemblies
----------
* StructureMap.Testing.GenericWidgets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget2, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget3, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget4, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget5, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Connect all implementations of open generic type IService<T>
* Find and register all types implementing StructureMap.Testing.Widget.IWidget

*/
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDidIScan_smoke_tester.cs#L43-L74' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_whatdidiscan-result-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The textual report will show:

1. All the scanning operations (calls to `Registry.Scan()`) with a descriptive name, either one supplied by you or the `Registry` type and an order number.
1. All the assemblies that were part of the scanning operation including the assembly name, version, and a warning if `Assembly.GetExportedTypes()` failed on that assembly.
1. All the configured scanning conventions inside of the scanning operation

`WhatDidIScan()` does not at this time show any type filters or exclusions that may be part of the assembly scanner.

See also: [Auto-Registration and Conventions](/guide/ioc/registration/auto-registration-and-conventions)
