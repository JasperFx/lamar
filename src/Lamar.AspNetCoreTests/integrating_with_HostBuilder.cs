using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using Lamar.IoC.Frames;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
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
                
                container.GetInstance<IServiceProviderIsService>()
                    .ShouldBeSameAs(container);

                container.GetInstance<IServiceVariableSource>()
                    .ShouldBeOfType<ServiceVariableSource>();
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
    }
}
