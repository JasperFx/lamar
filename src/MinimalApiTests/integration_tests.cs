using System;
using System.Threading.Tasks;
using Alba;
using Shouldly;
using Xunit;

namespace MinimalApiTests;

public class IntegrationFixture : IAsyncLifetime
{
    public IAlbaHost Host { get; private set; }
    
    public async Task InitializeAsync()
    {
        Host = await AlbaHost.For<global::Program>(x => {});
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

}

public class integration_tests : IClassFixture<IntegrationFixture>
{
    private readonly IntegrationFixture _fixture;

    public integration_tests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task can_execute_minimal_api_routes()
    {
        var text = await _fixture.Host.GetAsText("/");
        text.ShouldBe("Hi there");
    }

    [Fact]
    public async Task can_execute_a_controller_route_added_through_AddControllers_in_UseLamar()
    {
        var text = await _fixture.Host.GetAsText("/api/hello");
        text.ShouldStartWith("Hi there@");
    }
}