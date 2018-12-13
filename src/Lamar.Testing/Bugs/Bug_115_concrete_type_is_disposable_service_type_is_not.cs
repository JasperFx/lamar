using System;
using Baseline;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_115_concrete_type_is_disposable_service_type_is_not
    {
        public interface ITruck
        {
            IEngine Engine { get; }
        }

        public class Truck : ITruck
        {
            public Truck(IEngine engine)
            {
                Engine = engine;
            }

            public IEngine Engine { get; }
        }

        public class InternalTruck : Truck
        {
            public InternalTruck(IEngine engine) : base(engine)
            {
            }
        }

        public interface IEngine { }

        public class Engine : IEngine, IDisposable
        {
            public void Dispose()
            {
                WasDisposed = true;
            }

            public bool WasDisposed { get; set; }
        }

        class InternalEngine : Engine { }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void should_be_disposed_as_constructor_function(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<ITruck>().Use<Truck>().Lifetime = lifetime;
                _.For<IEngine>().Use<Engine>().Lifetime = lifetime;
            });

            var nested = container.GetNestedContainer();

            var truck = nested.GetInstance<ITruck>();

            var engine = truck.Engine;

            nested.Dispose();
            container.Dispose();

            engine.As<Engine>().WasDisposed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void should_be_disposed_when_registered_through_aspnet_core(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.Add(new ServiceDescriptor(typeof(ITruck), typeof(Truck), lifetime));
                _.Add(new ServiceDescriptor(typeof(IEngine), typeof(Engine), lifetime));
            });

            var nested = container.GetNestedContainer();

            var truck = nested.GetInstance<ITruck>();

            var engine = truck.Engine;

            nested.Dispose();
            container.Dispose();

            engine.As<Engine>().WasDisposed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void should_be_disposed_as_lambda_function(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<ITruck>().Use(c => new Truck(c.GetInstance<IEngine>())).Lifetime = lifetime;
                _.For<IEngine>().Use(c => new Engine()).Lifetime = lifetime;
            });

            var nested = container.GetNestedContainer();

            var truck = nested.GetInstance<ITruck>();

            var engine = truck.Engine;

            nested.Dispose();
            container.Dispose();

            engine.As<Engine>().WasDisposed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void should_be_disposed_as_constructor_function_internal(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<ITruck>().Use<InternalTruck>().Lifetime = lifetime;
                _.For<IEngine>().Use<InternalEngine>().Lifetime = lifetime;
            });

            var nested = container.GetNestedContainer();

            var truck = nested.GetInstance<ITruck>();

            var engine = truck.Engine;

            nested.Dispose();
            container.Dispose();

            engine.As<Engine>().WasDisposed.ShouldBeTrue();
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
                _.Add(new ServiceDescriptor(typeof(IEngine), typeof(InternalEngine), lifetime));
            });

            var nested = container.GetNestedContainer();

            var truck = nested.GetInstance<ITruck>();

            var engine = truck.Engine;

            nested.Dispose();
            container.Dispose();

            engine.As<Engine>().WasDisposed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void should_be_disposed_as_lambda_function_internal(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.For<ITruck>().Use(c => new InternalTruck(c.GetInstance<IEngine>())).Lifetime = lifetime;
                _.For<IEngine>().Use(c => new InternalEngine()).Lifetime = lifetime;
            });

            var nested = container.GetNestedContainer();

            var truck = nested.GetInstance<ITruck>();

            var engine = truck.Engine;

            nested.Dispose();
            container.Dispose();

            engine.As<Engine>().WasDisposed.ShouldBeTrue();
        }
    }
}