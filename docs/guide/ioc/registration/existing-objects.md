# Registering Existing Objects

It's frequently common to register existing objects with a Lamar `Container` and there are
overloads of the `ServiceRegistry.For().Use(object)` and `ServiceRegistry.For().Add(object)` methods to do just that:

<[sample:injecting-pre-built-object]>

Injecting an existing object into the `Container` makes it a de facto singleton, but the `Container` treats it with a
special scope called `ObjectLifecycle` if you happen to look into the [WhatDoIHave()](/guide/ioc/diagnostics/what-do-i-have) diagnostics.

Lamar will attempt to call the `IDisposable.Dispose()` on any objects that are directly injected into a `Container`
that implement `IDisposable` when the `Container` itself is disposed.
