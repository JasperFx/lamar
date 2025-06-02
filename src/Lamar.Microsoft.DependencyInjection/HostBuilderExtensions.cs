using System;
using System.Collections.Generic;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using Lamar.IoC;
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

#if NET8_0_OR_GREATER
        /// <summary>
        /// Use Lamar as the DI/IoC container for this application
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static HostApplicationBuilder UseLamar(this HostApplicationBuilder builder,
            Action<ServiceRegistry> configure = null) => builder.UseLamar(InstanceMapBehavior.Default, configure);

        /// <summary>
        /// Use Lamar as the DI/IoC container for this application
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="instanceMapBehavior"></param>
        /// <returns></returns>
        public static HostApplicationBuilder UseLamar(this HostApplicationBuilder builder,
            InstanceMapBehavior instanceMapBehavior, Action<ServiceRegistry> configure = null)
        {
            builder.Services.AddSingleton(c =>
                c.GetRequiredService<IContainer>().CreateServiceVariableSource());
            
            // This enables the usage of implicit services in Minimal APIs
            builder.Services.AddSingleton(s => (IServiceProviderIsService) s.GetRequiredService<IContainer>());
            builder.Services.AddSingleton(s => (IServiceProviderIsKeyedService) s.GetRequiredService<IContainer>());
            
            builder.ConfigureContainer<ServiceRegistry>(new LamarServiceProviderFactory(instanceMapBehavior), x =>
            {
                configure?.Invoke(x);
            });

            return builder;
        }
#endif

        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, Action<HostBuilderContext, ServiceRegistry> configure = null) =>
            builder.UseLamar(InstanceMapBehavior.Default, configure);

        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="instanceMapBehavior"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, InstanceMapBehavior instanceMapBehavior,
            Action<HostBuilderContext, ServiceRegistry> configure = null)
        {
            return builder
                .UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory(instanceMapBehavior))
                .UseServiceProviderFactory<IServiceCollection>(new LamarServiceProviderFactory(instanceMapBehavior))
                .ConfigureServices((context, services) =>
                {
                    var registry = new ServiceRegistry(services);
                
                    configure?.Invoke(context, registry);
                
                    services.Clear();
                    services.AddRange(registry);

                    services.AddSingleton(c =>
                        c.GetRequiredService<IContainer>().CreateServiceVariableSource());

#if NET6_0_OR_GREATER
                    // This enables the usage of implicit services in Minimal APIs
                    services.AddSingleton(s => (IServiceProviderIsService) s.GetRequiredService<IContainer>());
                    services.AddSingleton(s => (IServiceProviderIsKeyedService) s.GetRequiredService<IContainer>());
#endif
                });
        }

        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, Action<ServiceRegistry> configure) =>
            builder.UseLamar(InstanceMapBehavior.Default, configure);

        /// <summary>
        /// Shortcut to replace the built in DI container with Lamar using service registrations
        /// dependent upon the application's environment and configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <param name="instanceMapBehavior"></param>
        /// <returns></returns>
        public static IHostBuilder UseLamar(this IHostBuilder builder, InstanceMapBehavior instanceMapBehavior, Action<ServiceRegistry> configure)
        {
            return builder.UseLamar(instanceMapBehavior, (c, s) => configure.Invoke(s));
        }

        /// <summary>
        /// Overrides the internal DI container with Lamar, optionally using a Lamar ServiceRegistry
        /// for additional service registrations
        /// </summary>
        /// <param name="services"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IServiceCollection AddLamar(this IServiceCollection services, ServiceRegistry registry = null) =>
            services.AddLamar(InstanceMapBehavior.Default, registry);

        /// <summary>
        /// Overrides the internal DI container with Lamar, optionally using a Lamar ServiceRegistry
        /// for additional service registrations
        /// </summary>
        /// <param name="services"></param>
        /// <param name="registry"></param>
        /// <param name="instanceMapBehavior"></param>
        /// <returns></returns>
        public static IServiceCollection AddLamar(this IServiceCollection services, InstanceMapBehavior instanceMapBehavior, ServiceRegistry registry = null)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>(sp =>
                new LoggerFactory(
                    sp.GetRequiredService<IEnumerable<ILoggerProvider>>(),
                    sp.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>()
                ));
            services.AddSingleton<IServiceProviderFactory<ServiceRegistry>>(_ => new LamarServiceProviderFactory(instanceMapBehavior));
            services.AddSingleton<IServiceProviderFactory<IServiceCollection>>(_ => new LamarServiceProviderFactory(instanceMapBehavior));

            services.AddSingleton<IServiceVariableSource>(c =>
                c.GetRequiredService<IContainer>().CreateServiceVariableSource());

#if NET6_0_OR_GREATER
            services.AddSingleton<IServiceProviderIsService>(s => (IServiceProviderIsService) s.GetRequiredService<IContainer>());
            services.AddSingleton<IServiceProviderIsKeyedService>(s => (IServiceProviderIsKeyedService) s.GetRequiredService<IContainer>());
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