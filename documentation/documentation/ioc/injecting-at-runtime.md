<!--title:Injecting Services at Runtime-->

Lamar's predecessor [StructureMap](https://structuremap.github.io) allowed you to override service registrations in nested containers in a general way.
Some .Net application frameworks depend on this functionality to inject some kind of contextual service into a nested container. ASP.Net Core's `HttpContext` is an example. MassTransit's [ConsumeContext](https://github.com/MassTransit/MassTransit/blob/develop/src/MassTransit/ConsumeContext.cs) is another.

Using that as an example, let's say that our application framework has this context type that the framework creates and wants to inject directly
into nested containers for some kind of operation:

<[sample:ExecutionContext]>

We might well have a service in our code that is resolved from a Lamar container that depends on that `ExecutionContext` interface:

<[sample:ContextUsingService]>

The first thing we have to do is make a registration in Lamar **upfront** that lets the container know that `ExecutionContext` is going to be injected
at runtime:

<[sample:container-with-injectable]>

At runtime, we can inject `ExecutionContext` like this:

<[sample:injecting-context-to-nested]>

Finally, when we resolve a service that depends on `ExecutionContext` from the nested container
we built above, we can see that it has a reference to our context object:

<[sample:resolving-using-context]>

