using System;
using JasperFx.CodeGeneration.Model;
using Lamar.IoC.Frames;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Lamar.AspNetCoreTests;

#if NET8_0_OR_GREATER
public class integration_with_host_application_builder : IDisposable
{
    private readonly IHost theHost;

    public integration_with_host_application_builder()
    {
        theHost = Host.CreateApplicationBuilder()
            .UseLamar()
            .Build();
        
        theHost.Start();
    }

    public void Dispose()
    {
        theHost?.Dispose();
    }

    [Fact]
    public void should_be_using_lamar_as_the_container()
    {
        theHost.Services.ShouldBeOfType<Container>();
    }

    [Fact]
    public void should_register_IServiceProviderIsService()
    {
        theHost.Services.GetRequiredService<IServiceProviderIsService>()
            .ShouldBeOfType<Container>();
    }

    [Fact]
    public void should_register_IServiceVariableSource()
    {
        theHost.Services.GetRequiredService<IServiceVariableSource>()
            .ShouldBeOfType<ServiceVariableSource>();
    }
}
#endif