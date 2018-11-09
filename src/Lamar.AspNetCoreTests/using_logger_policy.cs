using System.IO;
using System.Linq;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Lamar.Testing.AspNetCoreIntegration
{
    public class Thing
    {
        public ILogger<Thing> Logger { get; }

        public Thing(ILogger<Thing> logger)
        {
            Logger = logger;
        }
    }
    
    public class using_logger_policy
    {
        [Fact]
        public void with_aspnet_core()
        {
            var builder = WebHost.CreateDefaultBuilder()
                
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                })
                .ConfigureLogging(x =>
            {
//                x.AddConsole();
//                x.AddDebug();
            })
                .UseStartup<Startup>()
                .UseLamar();
            
            var host = builder.Build();
            var services = host.Services;

            var options = services.GetService<IOptions<LoggerFilterOptions>>();
            var logging = options.Value;
            
            logging.ShouldBeSameAs(services.GetRequiredService<LoggerFilterOptions>());
            
            logging.Rules.Any().ShouldBeTrue();
            
            var logger = services.GetRequiredService<ILogger<Thing>>();
            
            
            logger.ShouldNotBeNull();
        }
        
        [Fact]
        public void is_a_singleton()
        {
            var container = new Container(x =>
            {
                x.Policies.Add<LoggerPolicy>();
                x.For<ILoggerFactory>().Use(new LoggerFactory());
            });

            var l1 = container.GetInstance<ILogger<Thing>>();
            var l2 = container.GetInstance<ILogger<Thing>>();

            var thingLogger1 = container.GetInstance<Thing>().Logger;



            var nested = container.GetNestedContainer();

            var l3 = nested.GetInstance<ILogger<Thing>>();
            var thingLogger2 = nested.GetInstance<Thing>().Logger;
            
            
            l1.ShouldBeSameAs(l2);
            l1.ShouldBeSameAs(thingLogger1);
            l1.ShouldBeSameAs(l3);
            l1.ShouldBeSameAs(thingLogger2);

        }
    }
}