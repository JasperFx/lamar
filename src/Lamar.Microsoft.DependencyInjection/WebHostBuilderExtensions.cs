using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            if (resolving == LoggingAndOptionResolving.Lamar)
            {
                services.AddSingleton<IRegistrationPolicy>(new LoggerPolicy());
                services.AddSingleton<IFamilyPolicy>(new LoggerPolicy());

                services.AddSingleton<IRegistrationPolicy>(new OptionsPolicy());
                services.AddSingleton<IFamilyPolicy>(new OptionsPolicy());
            }

            if (registry != null)
            {
                foreach (var descriptor in registry)
                {
                    services.Add(descriptor);
                }
            }

            return services;
        }
    }
}