using Lamar.IoC;
using Shouldly;
using System;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_398_service_registry_hash_collisions
{
    [Fact]
    public void GetHashCollisions_RegistryWithCollisions()
    {
        var registry = GetRegistryWithCollisions();

        var collisions = registry.GetInstanceHashCollisions();

        collisions.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetHashCollisions_RegistryWithoutCollisions()
    {
        var registry = GetRegistryWithoutCollisions();

        var collisions = registry.GetInstanceHashCollisions();

        collisions.ShouldBeEmpty();
    }

    [Fact]
    public void TryRemoveHashCollisions_RegistryWithCollisions()
    {
        var registry = GetRegistryWithCollisions();

        var removed = registry.TryRemoveInstanceHashCollisions();

        removed.ShouldBeTrue();
    }

    [Fact]
    public void TryRemoveHashCollisions_RegistryWithoutCollisions()
    {
        var registry = GetRegistryWithoutCollisions();

        var removed = registry.TryRemoveInstanceHashCollisions();

        removed.ShouldBeFalse();
    }

    [Fact]
    public void HandleInstanceHashCollisions()
    {
        var registry = GetRegistryWithCollisions();

        registry.TryRemoveInstanceHashCollisions();

        registry.GetInstanceHashCollisions().ShouldBeEmpty();

        var container = new Container(registry);
        container.GetInstance<IFoo>().ShouldBeOfType<Foo>();
        container.GetInstance<IBar>().ShouldBeOfType<Bar>();
    }

    [Fact]
    public void MitigateInstanceHashCollisions()
    {
        var registry = GetRegistryWithCollisions();

        registry.MitigateInstanceHashCollisions();

        registry.GetInstanceHashCollisions().ShouldBeEmpty();

        var container = new Container(registry);
        container.GetInstance<IFoo>().ShouldBeOfType<Foo>();
        container.GetInstance<IBar>().ShouldBeOfType<Bar>();
    }

    [Fact]
    public void MitigateInstanceHashCollisionsThrowsAfterLimitReached()
    {
        var registry = GetRegistryWithCollisions();

        var mitigateCollisions = () => registry.MitigateInstanceHashCollisions(0);

        mitigateCollisions.ShouldThrow<LamarInstanceHashCollisionException>();
    }

    [Fact]
    public void MitigateInstanceHashCollisionsUsesCustomRenamePolicy()
    {
        var registry = GetRegistryWithCollisions();

        var instanceFoo = registry.For<IFoo>().Use<Foo>();
        var instanceBar = registry.For<IBar>().Use<Bar>();

        instanceFoo.Hash = instanceBar.Hash = 1;

        registry.MitigateInstanceHashCollisions(instanceRenamePolicy: x => $"{x}.updated");

        instanceFoo.Name.ShouldEndWith(".updated");
        instanceBar.Name.ShouldEndWith(".updated");
    }

    [Fact]
    public void NamedCollisionThrows()
    {
        var registry = new ServiceRegistry();

        registry.For<IFoo>().Use<Foo>().Named("foo").Hash = 1;
        registry.For<IBar>().Use<Bar>().Named("bar").Hash = 1;

        Action handleCollisions = () => registry.TryRemoveInstanceHashCollisions();

        handleCollisions.ShouldThrow<LamarInstanceHashCollisionException>();
    }


    private ServiceRegistry GetRegistryWithCollisions()
    {
        var registry = new ServiceRegistry();

        registry.For<IFoo>().Use<Foo>().Hash = 1;
        registry.For<IBar>().Use<Bar>().Hash = 1;

        return registry;
    }

    private ServiceRegistry GetRegistryWithoutCollisions()
    {
        var registry = new ServiceRegistry();

        registry.For<IFoo>().Use<Foo>().Hash = 1;
        registry.For<IBar>().Use<Bar>().Hash = 2;

        return registry;
    }

    public interface IFoo { }
    public interface IBar { }
    public class Foo : IFoo { }
    public class Bar : IBar { }
}
