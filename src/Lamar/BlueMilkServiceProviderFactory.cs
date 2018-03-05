using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public class LamarServiceProviderFactory : IServiceProviderFactory<ServiceRegistry>, IServiceProviderFactory<IServiceCollection>
    {
        public ServiceRegistry CreateBuilder(IServiceCollection services)
        {
            var registry = new ServiceRegistry();
            registry.AddRange(services);

            return registry;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return new Container(containerBuilder);
        }

        public IServiceProvider CreateServiceProvider(ServiceRegistry containerBuilder)
        {
            return new Container(containerBuilder);
        }

        IServiceCollection IServiceProviderFactory<IServiceCollection>.CreateBuilder(IServiceCollection services)
        {
            return CreateBuilder(services);
        }
    }
    
    
}