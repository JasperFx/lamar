using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            
            return services;
        }
    }
}