# Type Scanning Diagnostics

Type scanning and conventional auto-registration is a very powerful feature in Lamar, but it has been frequently troublesome to users when things go wrong. To try to alleviate problems, Lamar has some functionality for detecting and diagnosing problems with type scanning, mostly related to Assembly's being missing.

## Assert for Assembly Loading Failures

At its root, most type scanning and auto-registration schemes in .Net frameworks rely on the [Assembly.GetExportedTypes()](https://msdn.microsoft.com/en-us/library/system.reflection.assembly.getexportedtypes%28v=vs.110%29.aspx) method. Unfortunately, that method can be brittle and fail whenever any dependency of that Assembly cannot be loaded into the current process, even if your application has no need for that dependency. In Lamar, you can use this method to assert the presence of any assembly load exceptions during type scanning:

<[sample:assert-no-type-scanning-failures]>

The method above will throw an exception listing all the Assembly's that failed during the call to `GetExportedTypes()` only if there were any failures. Use this method during your application bootstrapping if you want it to fail fast with any type scanning problems.

## What did Lamar scan?

Confusion of type scanning has been a constant problem with Lamar usage over the years -- especially if users are trying to dynamically load assemblies from the file system for extensibility. In order to see into what Lamar has done with type scanning, 4.0 introduces the `Container.WhatDidIScan()` method.

Let's say that you have a `Container` that is set up with at least two different scanning operations like this sample from the Lamar unit tests:

<[sample:whatdidiscan]>

The resulting textual report is shown below:

_Sorry for the formatting and color of the text, but the markdown engine does **not** like the textual report_
<[sample:whatdidiscan-result]>

The textual report will show:

1. All the scanning operations (calls to `Registry.Scan()`) with a descriptive name, either one supplied by you or the `Registry` type and an order number.
1. All the assemblies that were part of the scanning operation including the assembly name, version, and a warning if `Assembly.GetExportedTypes()` failed on that assembly.
1. All the configured scanning conventions inside of the scanning operation

`WhatDidIScan()` does not at this time show any type filters or exclusions that may be part of the assembly scanner.

See also: [Auto-Registration and Conventions](/guide/ioc/registration/auto-registration-and-conventions)
