﻿using Baseline;

using Lamar.Microsoft.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shouldly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.AspNetCoreIntegration
{
    public class integrating_with_HostBuilder
    {
        private readonly ITestOutputHelper _output;

        public integrating_with_HostBuilder(ITestOutputHelper output)
        {
            _output = output;
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
            public String Greeting { get; set; } = "Go Away!";
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
    }
}
