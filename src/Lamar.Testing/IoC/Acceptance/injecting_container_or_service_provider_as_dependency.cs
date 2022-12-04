using System;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class injecting_container_or_service_provider_as_dependency
{
    [Fact]
    public void container_from_root()
    {
        var container = Container.Empty();

        container.GetInstance<IContainer>().ShouldBe(container);
        container.GetInstance<GuyWithContainer>()
            .Container
            .ShouldBeSameAs(container);
    }

    [Fact]
    public void service_provider_from_root()
    {
        var container = Container.Empty();

        container.GetInstance<IServiceProvider>().ShouldBe(container);
        container.GetInstance<GuyWithServiceProvider>()
            .Provider
            .ShouldBeSameAs(container);
    }

    [Fact]
    public void scope_factory_from_root()
    {
        var container = Container.Empty();

        container.GetInstance<IServiceScopeFactory>().ShouldBe(container);
    }

    [Fact]
    public void container_from_nested()
    {
        var container = Container.Empty();
        var nested = container.GetNestedContainer();

        nested.GetInstance<IContainer>().ShouldBeSameAs(nested);
        nested.GetInstance<GuyWithContainer>()
            .Container
            .ShouldBeSameAs(nested);
    }

    [Fact]
    public void service_provider_from_nested()
    {
        var container = Container.Empty();
        var nested = container.GetNestedContainer();

        nested.GetInstance<IServiceProvider>().ShouldBeSameAs(nested);
        nested.GetInstance<GuyWithServiceProvider>()
            .Provider
            .ShouldBeSameAs(nested);
    }
}

public class GuyWithContainer
{
    public GuyWithContainer(IContainer container)
    {
        Container = container;
    }

    public IContainer Container { get; }
}

public class GuyWithServiceProvider
{
    public GuyWithServiceProvider(IServiceProvider provider)
    {
        Provider = provider;
    }

    public IServiceProvider Provider { get; }
}