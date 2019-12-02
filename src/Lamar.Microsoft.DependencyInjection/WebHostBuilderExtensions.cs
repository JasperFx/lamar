using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lamar.Microsoft.DependencyInjection
{

    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseLamar<T>(this IWebHostBuilder builder, Action<WebHostBuilderContext, T> configure = null) where T : ServiceRegistry, new()
        {
            return builder.ConfigureServices((context, services) =>
            {
                var registry = new T();
                configure?.Invoke(context, registry);

                services.AddLamar(registry);
            });
        }
        
        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, Action<WebHostBuilderContext, ServiceRegistry> configure = null)
        {
            return builder.UseLamar<ServiceRegistry>(configure);
        }
        
        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, ServiceRegistry registry)
        {
            return builder.ConfigureServices((context, services) => { services.AddLamar(registry); });
        }

        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using the extra service registrations
        /// in the ServiceRegistry
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, ServiceRegistry registry)
        {
            return 
                
                
                builder
                    .UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory())
                    .UseServiceProviderFactory<IServiceCollection>(new LamarServiceProviderFactory())
                    
                    .ConfigureServices((context, x) => x.AddLamar(registry));
            
        }
        
        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, Action<HostBuilderContext, ServiceRegistry> configure = null)
        {
            return builder.UseLamar<ServiceRegistry>(configure);
        }


        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using the extra service registrations
        /// in the "T" ServiceRegistry
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure">Add additional service registrations</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IHostBuilder UseLamar<T>(this IHostBuilder builder, Action<HostBuilderContext, T> configure = null) where T : ServiceRegistry, new()
        {
            return builder
                .UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory())
                .ConfigureServices((context, services) =>
            {
                var registry = new T();
                
                configure?.Invoke(context, registry);
                
                services.AddLamar(registry);
            });
        }
        


        /// <summary>
        /// Overrides the internal DI container with Lamar, optionally using a Lamar ServiceRegistry
        /// for additional service registrations
        /// </summary>
        /// <param name="services"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IServiceCollection AddLamar(this IServiceCollection services, ServiceRegistry registry = null)

        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>(sp =>
                new LoggerFactory(
                    sp.GetRequiredService<IEnumerable<ILoggerProvider>>(),
                    sp.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>()
                ));
            services.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
            services.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

            registry = registry ?? new ServiceRegistry();

            foreach (var descriptor in registry)
            {
                services.Add(descriptor);
            }

            return services;
        }
    }
}