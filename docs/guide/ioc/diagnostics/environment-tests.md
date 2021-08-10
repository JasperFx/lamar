# Environment Tests

Years ago I worked with a legacy system that was particularly fragile in its deployment. While my team at the time and I made some serious improvements in the reliability of the automated deployment, the best thing we did was to add a set of _environment tests_ to the deployment that verified that basic elements of the system were working like:

* Could our code access the configured database?
* Was a certain COM object registered on the server? (I hated COM then and the years haven't changed my mind)
* Could we connect via remoting to another deployed application?

The deployments still frequently failed, but we were able to spot **and diagnose** the underlying problems much faster with our new environment tests than we could before by trying to run and debug the not-quite-valid application.

One of the mechanisms we used for these environment tests was Lamar's ability to mark methods on configured types as environment tests with the `[ValidationMethod]` attribute as shown below:

<!-- snippet: sample_validation-method-usage -->
<a id='snippet-sample_validation-method-usage'></a>
```cs
public class Database : IDatabase
{
    [ValidationMethod]
    public void TryToConnect()
    {
        // try to open a connection to the configured
        // database connection string

        // throw an exception if the database cannot
        // be reached
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/ValidationMethod.cs#L12-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_validation-method-usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Used in conjunction with [Lamar's ability to validate a container](/guide/ioc/diagnostics/validating-container-configuration), you can use this technique to quickly support environment tests embedded into your system code.
