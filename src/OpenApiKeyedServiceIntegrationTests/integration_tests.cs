using System;
using System.Threading.Tasks;
using Alba;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Shouldly;
using Xunit;

namespace OpenApiKeyedServiceIntegrationTests;

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
    public void validate_keyed_services()
    {
        var container = (Container)_fixture.Host.Services.GetRequiredService<IContainer>();
        container.IsKeyedService(typeof(ITest), OpenApiTestConstants.TestServiceKey1).ShouldBeTrue();
        container.IsKeyedService(typeof(ITest), OpenApiTestConstants.TestServiceKey2).ShouldBeTrue();
        container.IsKeyedService(typeof(ITest), OpenApiTestConstants.TestServiceKey3).ShouldBeTrue();
        container.IsKeyedService(typeof(ITestTime), "ITestTime1").ShouldBeFalse();
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

    [Fact]
    public async Task can_execute_open_api_route()
    {
        var openApiDocument = await _fixture.Host.GetAsJson<OpenApiDocument>("/openapi/v1.json");

        openApiDocument.ShouldNotBeNull();
        openApiDocument.Info.Title.ShouldBe("LamarWithOpenApiOnNet9 | v1");
    }
}