using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class resolving_objects_passed_to_container
{
    [Fact]
    public void register_and_resolve_object_by_type()
    {
        var clock = new Clock();

        var container = Container.For(_ => { _.AddSingleton<IClock>(clock); });

        container.GetInstance<IClock>()
            .ShouldBeSameAs(clock);
    }

    [Fact]
    public void register_and_resolve_object_by_type_2()
    {
        var clock = new Clock();

        var container = Container.For(_ => { _.AddSingleton<IClock>(clock); });

        container.GetInstance(typeof(IClock))
            .ShouldBeSameAs(clock);
    }

    [Fact]
    public void register_and_resolve_object_by_type_and_name()
    {
        var red = new Clock();
        var green = new Clock();
        var blue = new Clock();

        var container = Container.For(_ =>
        {
            _.For<IClock>().Use(red).Named("red");
            _.For<IClock>().Use(green).Named("green");
            _.For<IClock>().Use(blue).Named("blue");
        });

        container.GetInstance<IClock>("red").ShouldBeSameAs(red);
        container.GetInstance<IClock>("green").ShouldBeSameAs(green);
        container.GetInstance<IClock>("blue").ShouldBeSameAs(blue);
    }

    [Fact]
    public void register_and_resolve_object_by_type_and_name_2()
    {
        var red = new Clock();
        var green = new Clock();
        var blue = new Clock();

        var container = Container.For(_ =>
        {
            _.For<IClock>().Use(red).Named("red");
            _.For<IClock>().Use(green).Named("green");
            _.For<IClock>().Use(blue).Named("blue");
        });

        container.GetInstance(typeof(IClock), "red").ShouldBeSameAs(red);
        container.GetInstance(typeof(IClock), "green").ShouldBeSameAs(green);
        container.GetInstance(typeof(IClock), "blue").ShouldBeSameAs(blue);
    }

    [Fact]
    public void registered_objects_are_disposed_when_the_container_is_disposed()
    {
        var disposable = new DisposableClock();

        var container = Container.For(_ => { _.For<IClock>().Use(disposable); });

        container.Dispose();

        disposable.WasDisposed.ShouldBeTrue();
    }
}