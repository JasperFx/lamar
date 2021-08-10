# Integration with Blazor

::: warning
No specific Blazor support has been built into Lamar, and Blazor is still an immature and rapidly-evolving framework. Use of Lamar within Blazor apps is considered experimental.
:::

To use Lamar within Blazor applications, you need only the base [Lamar](https://www.nuget.org/packages/Lamar/) NuGet package installed. Then, you can configure Blazor's host builder to use Lamar for IoC as shown below.

```csharp
using System.Threading.Tasks;
using Lamar;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Lamar.Sample.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // configure Blazor to use Lamar
            builder.ConfigureContainer<ServiceRegistry>(new LamarServiceProviderFactory(), ConfigureServices);

            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(ServiceRegistry services)
        {
            // here you can configure Lamar as normal

            services.For<IFoo>().Use<Foo>().Singleton();
            services.IncludeRegistry<FooRegistry>();
        }
    }
}
```

That's all you need; Blazor will now use Lamar for all dependency injection.
