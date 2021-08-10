# Building Objects with Lambdas

Instead of allowing Lamar to build objects directly, you can give a Lamar `Container` a [Lambda function](https://msdn.microsoft.com/en-us/library/bb397687.aspx) that can be called to create an object at resolution time.

Using NHibernate's [`ISession`](https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/ISession.cs) as an example
of an object that typically has to be built by using an [`ISessionFactory`](https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/ISessionFactory.cs) object:

<!-- snippet: sample_nhibernate-isession-factory -->
<a id='snippet-sample_nhibernate-isession-factory'></a>
```cs
public interface ISession { }

public interface ISessionFactory
{
    ISession Build();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/TalkSamples.cs#L52-L59' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_nhibernate-isession-factory' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If we want to allow Lamar to control the `ISession` lifecycle and creation, we have to register a Lambda function as the
means of creating `ISession` as shown in this example below:

<!-- snippet: sample_SessionFactoryRegistry -->
<a id='snippet-sample_sessionfactoryregistry'></a>
```cs
public class SessionFactoryRegistry : Registry
{
    // Let's not worry about how ISessionFactory is built
    // in this example
    public SessionFactoryRegistry(ISessionFactory factory)
    {
        For<ISessionFactory>().Use(factory);

        // Build ISession with a lambda:
        For<ISession>().Use("Build ISession from ISessionFactory", c =>
        {
            // To resolve ISession, I first pull out
            // ISessionFactory from the IContext and use that
            // to build a new ISession. 
            return c.GetInstance<ISessionFactory>().Build();
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Samples/TalkSamples.cs#L61-L80' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sessionfactoryregistry' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Lambda registrations can be done with any of the following four signatures:

1. `(Expression<Func<IContext, T>> builder)` -- a simple, one line Lambda to build `T` using `IContext`
1. `(Expression<Func<T>> func)` -- a simple, one line Lambda to build `T`
1. `(string description, Func<IContext, T> builder)` -- use `IContext` in your builder Lambda with a user-supplied description for diagnostics
1. `(string description, Func<T> builder)` -- Supply a complex `Func<T>` with a user-supplied description for diagnostics

**Be very wary of the difference between legal `Expression's` and more complicated Lambda's that will need to be `Func's`.** It likely doesn't matter to
you the user, but it unfortunately does to Lamar and .NET itself. If you need to use a more complex `Func`, you will have
to supply a diagnostic description.
