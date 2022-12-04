using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC;

public class optional_ctor_parameters
{
    [Fact]
    public void build_with_optional_primitive_argument_no_inline_value()
    {
        var container = new Container(x => { x.For<IClock>().Use<Clock>(); });

        container.GetInstance<IClock>()
            .ShouldBeOfType<Clock>()
            .Hours.ShouldBe(24);
    }

    [Fact]
    public void build_with_optional_primitive_argument_with_inline_value()
    {
        var container = new Container(x => { x.For<IClock>().Use<Clock>().Ctor<int>().Is(12); });

        container.GetInstance<IClock>()
            .ShouldBeOfType<Clock>()
            .Hours.ShouldBe(12);
    }


    [Fact]
    public void null_default_for_optional_constructor()
    {
        var container = Container.Empty();

        container.GetInstance<ClockUsingGuy>()
            .Clock.ShouldBeOfType<Clock>();
    }

    [Fact]
    public void override_default_with_inline()
    {
        var container = Container.For(x =>
        {
            x.For<ClockUsingGuy>().Use<ClockUsingGuy>().Ctor<IClock>().Is<AClock>();
        });

        container.GetInstance<ClockUsingGuy>()
            .Clock.ShouldBeOfType<AClock>();
    }

    [Fact]
    public void override_default_with_registered_service()
    {
        var container = Container.For(x => { x.For<IClock>().Use<AClock>(); });

        container.GetInstance<ClockUsingGuy>()
            .Clock.ShouldBeOfType<AClock>();
    }

    public class Clock : IClock
    {
        public Clock(int hours = 24)
        {
            Hours = hours;
        }

        public int Hours { get; }
    }

    public class AClock : IClock
    {
    }

    public interface IClock
    {
    }

    public class ClockUsingGuy
    {
        public ClockUsingGuy(IClock clock = null)
        {
            Clock = clock ?? new Clock();
        }

        public IClock Clock { get; }
    }
}