<!--title:Decorators-->

For version 1.0, Lamar's only support for interception is decorators. If you look in the Lamar codebase, you'll find dozens of tests that use a fake type named `IWidget`:

<[sample:IWidget]>

Let's say that we have service registrations in our system for that `IWidget` interface, but we want each of them to be decorated by another form of `IWidget` like this:

<[sample:WidgetHolder-Decorator]>

We can configure Lamar to add a decorator around all other `IWidget` registrations with this syntax:

<[sample:decorator-sample]>

