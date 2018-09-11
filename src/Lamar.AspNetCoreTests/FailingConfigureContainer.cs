using System;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StructureMap;
using StructureMap.AspNetCore;
using Xunit;

namespace Lamar.Testing.AspNetCoreIntegration
{
    public class FailingConfigureContainer
    {
        [Fact]
        public void lamar_in_app_configure_container_uses_decorator()
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseLamar()
                .UseStartup<FailingStartupLamar>();
            
            //FAILING TEST
            Assert.ThrowsAny<Exception>(() => builder.Start());
        }

        [Fact]
        public void structuremap_in_app_configure_container_uses_decorator()
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseStructureMap()
                .UseStartup<FailingStartupStructuremap>();

            Assert.ThrowsAny<Exception>(() => builder.Start());
        }
    }

    public class FailingStartupLamar
    {
        public void ConfigureContainer(ServiceRegistry registry)
        {
            registry.For(typeof(IOptionsFactory<>)).DecorateAllWith(typeof(OptionsFactoryDecorator<>));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app) => app.UseMvc();
    }

    public class FailingStartupStructuremap
    {
        public void ConfigureContainer(Registry registry)
        {
            registry.For(typeof(IOptionsFactory<>)).DecorateAllWith(typeof(OptionsFactoryDecorator<>));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app) => app.UseMvc();
    }

    public class OptionsFactoryDecorator<T> : IOptionsFactory<T> where T : class, new()
    {
        public T Create(string name)
        {
            throw new Exception("This exception should be thrown any time some one tries to access IOptions<>");
        }
    }
}