using System;
using Lamar.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Microsoft.DependencyInjection
{
    public class LamarServiceProviderFactory : IServiceProviderFactory<ServiceRegistry>, IServiceProviderFactory<IServiceCollection>
    {
        private readonly InstanceMapBehavior _instanceMapBehavior;

        public LamarServiceProviderFactory()
        {
            _instanceMapBehavior = InstanceMapBehavior.Default;
        }
        
        public LamarServiceProviderFactory(InstanceMapBehavior instanceMapBehavior)
        {
            _instanceMapBehavior  = instanceMapBehavior;
        }
        
        public ServiceRegistry CreateBuilder(IServiceCollection services)
        {
            var registry = new ServiceRegistry();
            registry.AddRange(services);

            return registry;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return new Container(containerBuilder, _instanceMapBehavior);
        }

        public IServiceProvider CreateServiceProvider(ServiceRegistry containerBuilder)
        {
            return new Container(containerBuilder, _instanceMapBehavior);
        }

        IServiceCollection IServiceProviderFactory<IServiceCollection>.CreateBuilder(IServiceCollection services)
        {
            return CreateBuilder(services);
        }
    }
}