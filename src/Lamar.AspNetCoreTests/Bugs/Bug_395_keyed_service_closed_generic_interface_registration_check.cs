using System.Collections.Generic;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Lamar.AspNetCoreTests.Bugs;

public class Bug_395_keyed_service_closed_generic_interface_registration_check
{
    class ClassA {}
    
    interface IGenericService<T> {}
    
    class ServiceA : IGenericService<ClassA> {}
    
    [Fact]
    public void do_not_blow_up()
    {
        var serviceName = "MyServiceName";
        
        var builder = new HostBuilder()
            .ConfigureServices((context, services) =>
            {
                // This is needed because of https://github.com/aspnet/Logging/issues/691
                services.AddSingleton<ILoggerFactory, LoggerFactory>(sp =>
                    new LoggerFactory(
                        sp.GetRequiredService<IEnumerable<ILoggerProvider>>(),
                        sp.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>()
                    )
                );

                services.AddKeyedTransient<IGenericService<ClassA>, ServiceA>(serviceName);
            })
            .UseLamar();

        using (var host = builder.Start())
        {
            var container = host.Services.ShouldBeOfType<Container>();
            
            container.GetInstance<IGenericService<ClassA>>()
                .ShouldNotBeNull();
        }
    }
}