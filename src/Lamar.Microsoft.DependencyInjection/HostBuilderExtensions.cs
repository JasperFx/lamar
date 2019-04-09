using Microsoft.Extensions.Hosting;

namespace Lamar.Microsoft.DependencyInjection
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseLamar(this IHostBuilder builder)
        {
            return UseLamar(builder, LoggingAndOptionResolving.Lamar);
        }

        public static IHostBuilder UseLamar(this IHostBuilder builder, LoggingAndOptionResolving resolving)
        {
            return UseLamar(builder, registry: null, resolving: resolving);
        }

        public static IHostBuilder UseLamar(this IHostBuilder builder, ServiceRegistry registry)
        {
            return UseLamar(builder, registry: registry, resolving: LoggingAndOptionResolving.Lamar);
        }

        public static IHostBuilder UseLamar(this IHostBuilder builder, ServiceRegistry registry, LoggingAndOptionResolving resolving)
        {
            return builder
                .UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory())
                .ConfigureServices((context, services) => { services.AddLamar(registry, resolving); });
        }
    }
}
