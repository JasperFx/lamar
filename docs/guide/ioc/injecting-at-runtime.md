# Injecting Services at Runtime

Lamar's predecessor [StructureMap](https://structuremap.github.io) allowed you to override service registrations in nested containers in a general way. Some .Net application frameworks depend on this functionality to inject some kind of contextual service into a nested container. ASP.Net Core's `HttpContext` is an example. MassTransit's [ConsumeContext](https://github.com/MassTransit/MassTransit/blob/develop/src/MassTransit/ConsumeContext.cs) is another.

Using that as an example, let's say that our application framework has this context type that the framework creates and wants to inject directly
into nested containers for some kind of operation:

<!-- snippet: sample_ExecutionContext -->
<a id='snippet-sample_executioncontext'></a>
```cs
// This class is specific to some kind of short lived 
// process and lives in a nested container
public class ExecutionContext
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/inject_to_scope.cs#L191-L198' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_executioncontext' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

We might well have a service in our code that is resolved from a Lamar container that depends on that `ExecutionContext` interface:

<!-- snippet: sample_ContextUsingService -->
<a id='snippet-sample_contextusingservice'></a>
```cs
public class ContextUsingService
{
    public ExecutionContext Context { get; }

    public ContextUsingService(ExecutionContext context)
    {
        Context = context;
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/inject_to_scope.cs#L179-L189' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_contextusingservice' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The first thing we have to do is make a registration in Lamar **upfront** that lets the container know that `ExecutionContext` is going to be injected
at runtime:

<!-- snippet: sample_container-with-injectable -->
<a id='snippet-sample_container-with-injectable'></a>
```cs
var container = new Container(_ =>
{
    _.Injectable<ExecutionContext>();
});
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/inject_to_scope.cs#L155-L160' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_container-with-injectable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

At runtime, we can inject `ExecutionContext` like this:

<!-- snippet: sample_injecting-context-to-nested -->
<a id='snippet-sample_injecting-context-to-nested'></a>
```cs
var context = new ExecutionContext();

var nested = container.GetNestedContainer();
nested.Inject(context);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/inject_to_scope.cs#L162-L167' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_injecting-context-to-nested' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Finally, when we resolve a service that depends on `ExecutionContext` from the nested container
we built above, we can see that it has a reference to our context object:

<!-- snippet: sample_resolving-using-context -->
<a id='snippet-sample_resolving-using-context'></a>
```cs
var service = nested.GetInstance<ContextUsingService>();
service.Context.ShouldBeSameAs(context);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/inject_to_scope.cs#L170-L173' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_resolving-using-context' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
