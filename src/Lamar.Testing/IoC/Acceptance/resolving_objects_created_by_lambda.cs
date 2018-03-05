using System;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class resolving_objects_created_by_lambda
    {
        public resolving_objects_created_by_lambda()
        {
            ClockFactory.Number = 0;
        }
        
        [Fact]
        public void build_with_container()
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<BlueWidget>().Named("Blue");
                _.For<IWidget>().Use<RedWidget>();
                _.For<WidgetUser>().Use(c => new WidgetUser(c.GetInstance<IWidget>("Blue")));
            });

            container.GetInstance<WidgetUser>()
                .Widget.ShouldBeOfType<BlueWidget>();

        }

        [Fact]
        public void register_as_transient()
        {
            var container = Container.For(_ =>
            {
                _.For<IClockFactory>().Use(new ClockFactory());
                _.AddTransient(s => s.GetService<IClockFactory>().Build());
            });

            var clock1 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock2 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock3 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            
            clock1.Number.ShouldBe(1);
            clock2.Number.ShouldBe(2);
            clock3.Number.ShouldBe(3);
        }
        
        [Fact]
        public void register_as_singleton()
        {
            var container = Container.For(_ =>
            {
                _.For<IClockFactory>().Use(new ClockFactory());
                _.AddSingleton(s => s.GetService<IClockFactory>().Build());
            });

            var clock1 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock2 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock3 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            
            clock1.Number.ShouldBe(1);
            clock2.Number.ShouldBe(1);
            clock3.Number.ShouldBe(1);
        }
        
        [Fact]
        public void register_as_scoped()
        {
            var container = Container.For(_ =>
            {
                _.For<IClockFactory>().Use(new ClockFactory());
                _.AddScoped(s => s.GetService<IClockFactory>().Build());
            });

            var clock1 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock2 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock3 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            
            clock1.Number.ShouldBe(1);
            clock2.Number.ShouldBe(1);
            clock3.Number.ShouldBe(1);
        }
        
        [Fact]
        public void should_dispose_of_built_values()
        {
            var container = Container.For(_ =>
            {
                _.For<IClockFactory>().Use(new ClockFactory());
                _.AddScoped(s => s.GetService<IClockFactory>().Build());
            });

            var clock1 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock2 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            var clock3 = container.GetInstance<IClock>().ShouldBeOfType<NumberedClock>();
            
            container.Dispose();
            
            clock1.WasDisposed.ShouldBeTrue();
            clock2.WasDisposed.ShouldBeTrue();
            clock3.WasDisposed.ShouldBeTrue();
        }
    }

    public interface IClockFactory
    {
        IClock Build();
    }

    public class ClockFactory : IClockFactory
    {
        public static int Number = 0;
        
        public IClock Build()
        {
            return new NumberedClock(++Number);
        }
    }

    public class NumberedClock : IClock, IDisposable
    {
        public int Number { get; }

        public NumberedClock(int number)
        {
            Number = number;
        }

        public DateTime Now()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }
    }
}