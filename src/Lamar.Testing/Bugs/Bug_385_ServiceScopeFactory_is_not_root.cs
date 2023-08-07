namespace Lamar.Testing.Bugs;

using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

public class Bug_385_ServiceScopeFactory_is_not_root
{
    [Fact]
    public void Test()
    {
        var container = Container.For(x =>
        {
            x.Use<TestService>();
        });

        var serviceScopeFactory = container.GetInstance<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var testService = scope.ServiceProvider.GetRequiredService<TestService>();

        testService.ServiceScopeFactory.ShouldBe(serviceScopeFactory);
    }

    public class TestService
    {
        public IServiceScopeFactory ServiceScopeFactory { get; }

        public TestService(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }
    }
}