using System;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lamar.Testing.AspNetCoreIntegration
{
    public class DefaultPolicyOverrideTests
    {
        [Fact]
        public void lamar_in_app_configure_container_uses_default_options_policy()
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseLamar()
                .UseStartup<FailingStartup>();

            //Does not throw since default options policy is used
            builder.Start();
        }

        [Fact]
        public void lamar_in_app_configure_container_uses_decorated_options()
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseLamar(addDefaultPolicies: false)
                .UseStartup<FailingStartup>();

            Assert.ThrowsAny<Exception>(() => builder.Start());
        }
    }

    public class FailingStartup
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

    public class OptionsFactoryDecorator<T> : IOptionsFactory<T> where T : class, new()
    {
        public T Create(string name)
        {
            throw new Exception("This exception should be thrown any time some one tries to access IOptions<>");
        }
    }
}