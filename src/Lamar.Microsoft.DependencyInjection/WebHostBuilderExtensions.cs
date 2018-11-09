using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lamar.Microsoft.DependencyInjection
{
    public enum  LoggingAndOptionResolving
    {
        /// <summary>
        /// Resolve IOptions<T> and ILogger<T> with idiomatic constructor construction. Use this if you
        /// are trying to decorate the ILogger interface
        /// </summary>
        AspNetCore,
        
        /// <summary>
        /// Use Lamar's optimized IOptions<T> and ILogger<T> resolution policies
        /// </summary>
        Lamar
    }
    
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder)
        {
            return UseLamar(builder, LoggingAndOptionResolving.Lamar);
        }
        
        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, LoggingAndOptionResolving resolving)
        {
            return UseLamar(builder, registry: null, resolving:resolving);
        }

        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, ServiceRegistry registry)
        {
            return UseLamar(builder, registry: registry, resolving:LoggingAndOptionResolving.Lamar);
        }

        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, ServiceRegistry registry, LoggingAndOptionResolving resolving)
        {
            return builder.ConfigureServices(services => { services.AddLamar(registry, resolving); });
        }

        public static IServiceCollection AddLamar(this IServiceCollection services)
        {
            return AddLamar(services, registry: null, resolving: LoggingAndOptionResolving.Lamar);
        }

        public static IServiceCollection AddLamar<T>(this IServiceCollection services, LoggingAndOptionResolving resolving = LoggingAndOptionResolving.Lamar) where T : ServiceRegistry, new()
        {
            return services.AddLamar(new T(), resolving);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, LoggingAndOptionResolving resolving)
        {
            return AddLamar(services, registry: null, resolving: resolving);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, ServiceRegistry registry)
        {
            return AddLamar(services, registry: registry, resolving: LoggingAndOptionResolving.Lamar);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, ServiceRegistry registry,
            LoggingAndOptionResolving resolving)

        {
            services.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
            services.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

            registry = registry ?? new ServiceRegistry();

            registry.For<LoggerFilterOptions>().Use(c => c.GetInstance<IOptions<LoggerFilterOptions>>().Value);
            
            if (resolving == LoggingAndOptionResolving.Lamar)
            {
                registry.Policies.Add(new LoggerPolicy());
                registry.Policies.Add(new OptionsPolicy());
            }

            foreach (var descriptor in registry)
            {
                services.Add(descriptor);
            }

            return services;
        }
    }
}