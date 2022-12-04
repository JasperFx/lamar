using System;
using Lamar.IoC;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class resolve_the_current_scope_and_container
{
    private readonly Container theContainer;

    public resolve_the_current_scope_and_container()
    {
        theContainer = new Container(_ =>
        {
            _.AddSingleton(new AWidget());
            _.AddSingleton<Thing>();
        });
    }

    [Theory]
    [InlineData(typeof(Scope))]
    [InlineData(typeof(IContainer))]
    [InlineData(typeof(IServiceProvider))]
    public void can_resolve_scope_from_root(Type serviceType)
    {
        theContainer.GetInstance(serviceType).ShouldBe(theContainer);
    }

    [Theory]
    [InlineData(typeof(Scope))]
    [InlineData(typeof(IContainer))]
    [InlineData(typeof(IServiceProvider))]
    public void can_resolve_scope_from_nested_scope(Type serviceType)
    {
        var scope = theContainer.CreateScope();

        scope.ServiceProvider.GetService(serviceType).ShouldBe(scope);
    }
}