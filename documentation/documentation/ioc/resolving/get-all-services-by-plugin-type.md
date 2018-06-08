<!--Title: Get all Services by Plugin Type-->
<!--Url: get-all-services-by-plugin-type-->


<[warning]>
The functionality respects the order in which the actual instances are configured in the Container -- which is compliant with the
expected behavior inside of ASP.Net Core.  Be warned that some other IoC tools make different assumptions if you are coming from a different tool.
<[/warning]>

Please see <[linkto:documentation/ioc/working-with-enumerable-types]> for a lot more information about what's going on behind the
scenes.

Once in a while you might want to get an enumerable of all the configured objects for a PluginType.  That's done with the `GetAllInstances()` method shown below:

<[sample:get-all-instances]>
