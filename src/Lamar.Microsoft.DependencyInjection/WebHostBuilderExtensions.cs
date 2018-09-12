using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lamar.Microsoft.DependencyInjection
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder)
        {
            return UseLamar(builder, registry: null, addDefaultPolicies: true);
        }

        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, bool addDefaultPolicies)
        {
            return UseLamar(builder, registry: null, addDefaultPolicies: addDefaultPolicies);
        }

        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, ServiceRegistry registry)
        {
            return UseLamar(builder, registry: registry, addDefaultPolicies: true);
        }

        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, ServiceRegistry registry, bool addDefaultPolicies)
        {
            return builder.ConfigureServices(services => { services.AddLamar(registry, addDefaultPolicies); });
        }

        public static IServiceCollection AddLamar(this IServiceCollection services)
        {
            return AddLamar(services, registry: null, addDefaultPolicies: true);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, bool addDefaultPolicies)
        {
            return AddLamar(services, registry: null, addDefaultPolicies: addDefaultPolicies);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, ServiceRegistry registry)
        {
            return AddLamar(services, registry: registry, addDefaultPolicies: true);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, ServiceRegistry registry, bool addDefaultPolicies)

        {
            services.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
            services.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

            if (addDefaultPolicies)
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