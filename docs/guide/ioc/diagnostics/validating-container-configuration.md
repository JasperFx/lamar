# Validating Container Configuration

To find any potential holes in your Lamar configuration like missing dependencies, unclear defaults of service types, validation errors, or just plain build errors, you can use this method below:

<!-- snippet: sample_container.AssertConfigurationIsValid -->
<a id='snippet-sample_container.assertconfigurationisvalid'></a>
```cs
container.AssertConfigurationIsValid();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Diagnostics/AssertConfigurationIsValid_Smoke_Tester.cs#L15-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_container.assertconfigurationisvalid' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Running this method will walk over every single registration in your `Container` and:

1. Try to create a build plan for that Instance that will flush out any problems with missing dependencies or invalid inline configuration
1. Try to build every single configured Instance
1. Call any methods on built objects decorated with the `[ValidationMethod]` attribute to perform environment tests. See [environment tests](/guide/ioc/diagnostics/environment-tests) for more information on this usage.

If Lamar encounters any errors of any kind during this method, it will throw an exception summarizing **all** of the problems that it encountered.
