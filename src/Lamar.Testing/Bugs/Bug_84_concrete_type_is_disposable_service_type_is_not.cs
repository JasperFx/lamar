using System;
using JasperFx.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_84_concrete_type_is_disposable_service_type_is_not
{
    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    public void should_be_disposed_as_constructor_function(ServiceLifetime lifetime)
    {
        var container = Container.For(_ => { _.For<ITruck>().Use<Truck>().Lifetime = lifetime; });

        var nested = container.GetNestedContainer();

        var truck = nested.GetInstance<ITruck>();

        nested.Dispose();
        container.Dispose();

        truck.As<Truck>().WasDisposed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    public void should_be_disposed_when_registered_through_aspnet_core(ServiceLifetime lifetime)
    {
        var container = Container.For(_ => { _.Add(new ServiceDescriptor(typeof(ITruck), typeof(Truck), lifetime)); });

        var nested = container.GetNestedContainer();

        var truck = nested.GetInstance<ITruck>();

        nested.Dispose();
        container.Dispose();

        truck.As<Truck>().WasDisposed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    public void should_be_disposed_as_lambda_function(ServiceLifetime lifetime)
    {
        var container = Container.For(_ => { _.For<ITruck>().Use(c => new Truck()).Lifetime = lifetime; });

        var nested = container.GetNestedContainer();

        var truck = nested.GetInstance<ITruck>();

        nested.Dispose();
        container.Dispose();

        truck.As<Truck>().WasDisposed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    public void should_be_disposed_as_constructor_function_internal(ServiceLifetime lifetime)
    {
        var container = Container.For(_ => { _.For<ITruck>().Use<InternalTruck>().Lifetime = lifetime; });

        var nested = container.GetNestedContainer();

        var truck = nested.GetInstance<ITruck>();

        nested.Dispose();
        container.Dispose();

        truck.As<Truck>().WasDisposed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    public void should_be_disposed_when_registered_through_aspnet_core_internal(ServiceLifetime lifetime)
    {
        var container = Container.For(_ =>
        {
            _.Add(new ServiceDescriptor(typeof(ITruck), typeof(InternalTruck), lifetime));
        });

        var nested = container.GetNestedContainer();

        var truck = nested.GetInstance<ITruck>();

        nested.Dispose();
        container.Dispose();

        truck.As<Truck>().WasDisposed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    public void should_be_disposed_as_lambda_function_internal(ServiceLifetime lifetime)
    {
        var container = Container.For(_ => { _.For<ITruck>().Use(c => new InternalTruck()).Lifetime = lifetime; });

        var nested = container.GetNestedContainer();

        var truck = nested.GetInstance<ITruck>();

        nested.Dispose();
        container.Dispose();

        truck.As<Truck>().WasDisposed.ShouldBeTrue();
    }

    public interface ITruck
    {
    }

    public class Truck : ITruck, IDisposable
    {
        public bool WasDisposed { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public class InternalTruck : Truck
    {
    }
}