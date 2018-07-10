using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging;
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