using System;
using System.Collections.Generic;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lamar.Microsoft.DependencyInjection
{

    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Apply Lamar service overrides regardless of the order of .Net service registrations. This is primarily
        /// meant for test automation scenarios
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides"></param>
        /// <returns></returns>
        public static IHostBuilder OverrideServices(this IHostBuilder builder, Action<ServiceRegistry> overrides)
        {
            return builder.ConfigureServices(x => x.OverrideServices(overrides));
        }
        


        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, Action<HostBuilderContext, ServiceRegistry> configure = null)
        {
            return builder
                .UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory())
                .UseServiceProviderFactory<IServiceCollection>(new LamarServiceProviderFactory())
                .ConfigureServices((context, services) =>
                {
                    var registry = new ServiceRegistry(services);
                
                    configure?.Invoke(context, registry);
                
                    services.Clear();
                    services.AddRange(registry);

#if NET6_0_OR_GREATER
                    services.AddSingleton<IServiceProviderIsService>(s => (IServiceProviderIsService) s.GetRequiredService<IContainer>());
#endif
                    
                });
        }
        
        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, Action<ServiceRegistry> configure)
        {
            return builder.UseLamar((c, s) => configure.Invoke(s));
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

#if NET6_0_OR_GREATER
            services.AddSingleton<IServiceProviderIsService>(s => (IServiceProviderIsService) s.GetRequiredService<IContainer>());
#endif

            registry ??= new ServiceRegistry();

            foreach (var descriptor in registry)
            {
                services.Add(descriptor);
            }

            return services;
        }
    }
}