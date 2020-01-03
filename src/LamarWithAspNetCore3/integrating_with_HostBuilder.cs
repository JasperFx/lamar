using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.AspNetCoreTests
{
    public class integrating_with_HostBuilder
    {
        private readonly ITestOutputHelper _output;

        public integrating_with_HostBuilder(ITestOutputHelper output)
        {
            _output = output;
        }

        public class CustomRegistry : ServiceRegistry
        {
            public CustomRegistry()
            {
                For<IWidget>().Use<AWidget>();
            }
        }

        public class DefaultRegistry : ServiceRegistry
        {
            public DefaultRegistry()
            {
                Scan(_ =>
                {
                    _.TheCallingAssembly();
                    _.ConnectImplementationsToTypesClosing(typeof(IInterface<,>));
                });
            }

        }

        public interface IInterface<T, P> { }
        public abstract class BaseClass<T, P> : IInterface<T, P> { }
        // If you comment out TestClass, the test passes
        public class TestClass<K> : BaseClass<SomeType, bool> { }
        public class SomeType { }

        [Fact]
        public void open_generic_types_issue()
        {
            var builder = new HostBuilder().UseLamar<DefaultRegistry>();
            using (var host = builder.Build())
            {
            }
        }

        [Fact]
        public void use_lamar_with_registry_type()
        {
            var builder = new HostBuilder()
                .UseLamar<CustomRegistry>();

            using (var host = builder.Build())
            {
                host.Services.ShouldBeOfType<Container>()
                    .Model.DefaultTypeFor<IWidget>()
                    .ShouldBe(typeof(AWidget));
            }
        }

        [Fact]
        public void use_lamar_with_HostBuilder()
        {
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
                })
                .UseLamar();

            using (var host = builder.Start())
            {
                var container = host.Services.ShouldBeOfType<Container>();
            }
        }

        [Fact]
        public void can_assert_configuration_is_valid_config_only()
        {
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
                })
                .UseLamar();

            using (var host = builder.Start())
            {
                var container = host.Services.ShouldBeOfType<Container>();

                var errors = container.Model.AllInstances.Where(x => x.Instance.ErrorMessages.Any())
                    .SelectMany(x => x.Instance.ErrorMessages).ToArray();

                if (errors.Any())
                {
                    throw new Exception(errors.Join(", "));
                }

                container.AssertConfigurationIsValid(AssertMode.ConfigOnly);
            }
        }

        [Fact]
        public void can_assert_configuration_is_valid_config_full()
        {
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
                })
                .UseLamar();

            using (var host = builder.Start())
            {
                var container = host.Services.ShouldBeOfType<Container>();

                var errors = container.Model.AllInstances.Where(x => x.Instance.ErrorMessages.Any())
                    .SelectMany(x => x.Instance.ErrorMessages).ToArray();

                if (errors.Any())
                {
                    throw new Exception(errors.Join(", "));
                }

                container.AssertConfigurationIsValid(AssertMode.Full);
            }
        }

        [Fact]
        public void can_initialize_from_ServiceRegistry()
        {
            var builder = new HostBuilder()
                .UseLamar(new MyServiceRegistry())
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", false);
                })
                .ConfigureServices((context, services) =>
                {
                    // This is needed because of https://github.com/aspnet/Logging/issues/691
                    services.AddSingleton<ILoggerFactory, LoggerFactory>(sp =>
                        new LoggerFactory(
                            sp.GetRequiredService<IEnumerable<ILoggerProvider>>(),
                            sp.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>()
                        )
                    );

                    services.Configure<MyServiceConfig>(context.Configuration.GetSection(nameof(MyServiceConfig)));
                })
                .ConfigureContainer<ServiceRegistry>((context, services) =>
                {
                });

            using (var host = builder.Start())
            {
                var container = host.Services.ShouldBeOfType<Container>();
                var service = host.Services.GetService<IHostedService>().ShouldBeOfType<MyServiceImpl>();

                service.Greeting.ShouldBe("Hello World!");
            }
        }

        [Fact]
        public void use_setter_injection_with_controller()
        {
            using (var host = Host.CreateDefaultBuilder()
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<LamarStartup>();
                }).Build())
            {
                // Do it the idiomatic Lamar way first
                var container = host.Services.As<IContainer>();

                var text = container.WhatDoIHave(serviceType:typeof(ISetter));
                
                var controller = container.GetInstance<WeatherForecastController>();
                controller.setter.ShouldNotBeNull();
            }
        }

        public class LamarStartup
        {
            public void ConfigureContainer(ServiceRegistry services)
            {
                services.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<ISetter>();
                    //scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                    scanner.SingleImplementationsOfInterface();
                });
            
                services.Policies.SetAllProperties(y => y.OfType<ISetter>());
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                // debug
                var container = (IContainer)app.ApplicationServices;
                var plan = container.Model.For<WeatherForecastController>().Default.DescribeBuildPlan();
                Console.WriteLine(plan); // this shows both inline AND setter in the build plan

                app.UseRouting();

                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }

        public class MyServiceRegistry : ServiceRegistry
        {
            public MyServiceRegistry()
            {
                Scan(scan =>
                {
                    scan.AssemblyContainingType<MyServiceImpl>();
                    scan.WithDefaultConventions();
                    scan.AddAllTypesOf<IHostedService>();
                });
            }
        }

        public class MyServiceConfig
        {
            public String Greeting { get; set; } = "Hello World!";
        }

        public class MyServiceImpl : BackgroundService
        {
            private readonly MyServiceConfig _options;
            private readonly ILogger<MyServiceImpl> _logger;

            public String Greeting => _options.Greeting;

            public MyServiceImpl(IOptions<MyServiceConfig> options, ILogger<MyServiceImpl> logger)
            {
                _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation("Greeting: {Greeting}", _options.Greeting);
                return Task.CompletedTask;
            }
        }
        
        public interface ISetter
        {

        }

        public class Setter : ISetter
        {

        }
        
        public class WeatherForecastController : ControllerBase
        {
            [SetterProperty] // doesn't help
            public ISetter setter { get; set; } // always null, with, or without attribute

            public WeatherForecastController() // this works
            {

            }
        }
        
        [ApiController]
        [Route("[controller]")]
        public class MyController : ControllerBase
        {
            [HttpGet("helloworld")]
            public string GetHelloWorld()
            {
                return "Hello World!";
            }
        }
        
        public class MyControllerRegistry : ServiceRegistry
        {
            public MyControllerRegistry()
            {				
                // this fails
                this.AddControllers();
            }			
        }

    }
}
