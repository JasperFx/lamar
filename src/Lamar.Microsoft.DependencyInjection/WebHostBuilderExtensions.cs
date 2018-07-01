using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lamar.Microsoft.DependencyInjection
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder)
        {
            return UseLamar(builder, registry: null);
        }

        public static IWebHostBuilder UseLamar(this IWebHostBuilder builder, ServiceRegistry registry)
        {
            return builder.ConfigureServices(services => { services.AddLamar(registry); });
        }
        
        public static IServiceCollection AddLamar(this IServiceCollection services)
        {
            return AddLamar(services, registry: null);
        }

        public static IServiceCollection AddLamar(this IServiceCollection services, 
            ServiceRegistry registry)
        
        {
            services.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
            services.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

            services.AddSingleton<IRegistrationPolicy>(new LoggerPolicy());
            services.AddSingleton<IFamilyPolicy>(new LoggerPolicy());
            
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