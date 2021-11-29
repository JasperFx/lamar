using System.Threading.Tasks;
#if NET6_0
using Alba;
#endif
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Rest.TransientFaultHandling;
using Shouldly;
using Xunit;

namespace Lamar.AspNetCoreTests
{
#if NET6_0
    public class usage_with_web_application_and_minimal_api
    {
        [Fact]
        public async Task spin_it_up_and_use_with_implied_services()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Host
                .UseLamar((c, services) =>
                {
                    services.For<ITest>().Use<MyTest>();
                });

            using var host = await AlbaHost.For(builder, app =>
            {
                app.MapGet("/", (ITest service) => service.SayHello());
            });

            var text= await host.GetAsText("/");
            text.ShouldBe("Hi there");
        }
    }
    
    public interface ITest
    {
        string SayHello();
    }

    public class MyTest : ITest
    {
        public string SayHello() => "Hi there";
    }

#endif
}