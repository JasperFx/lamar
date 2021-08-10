# Validating Container Configuration

To find any potential holes in your Lamar configuration like missing dependencies, unclear defaults of plugin types, validation errors, or just plain build errors, you can use this method below:

<[sample:container.AssertConfigurationIsValid]>

Running this method will walk over every single registration in your `Container` and:

1. Try to create a build plan for that Instance that will flush out any problems with missing dependencies or invalid inline configuration
1. Try to build every single configured Instance
1. Call any methods on built objects decorated with the `[ValidationMethod]` attribute to perform environment tests. See [environment tests](/guide/ioc/diagnostics/environment-tests) for more information on this usage.

If Lamar encounters any errors of any kind during this method, it will throw an exception summarizing **all** of the problems that it encountered.
