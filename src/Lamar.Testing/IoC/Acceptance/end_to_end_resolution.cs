using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public interface IThing
{
}

public class Thing : IThing
{
}

public class end_to_end_resolution
{
    [Fact]
    public void two_deep_transient()
    {
        var container = Container.For(_ =>
        {
            _.For<IWidget>().Use<WidgetWithThing>();
            _.For<IThing>().Use<Thing>();
            _.AddTransient<GuyWithWidget>();
        });

        container.GetInstance<GuyWithWidget>()
            .Widget.ShouldBeOfType<WidgetWithThing>()
            .Thing.ShouldBeOfType<Thing>();
    }

    [Fact]
    public void resolve_singletons_via_constructor()
    {
        var container = Container.For(_ =>
        {
            _.For<IWidget>().Use<WidgetWithThing>();
            _.For<IThing>().Use<Thing>();
            _.AddSingleton<GuyWithWidget>();
        });

        container.GetInstance<GuyWithWidget>()
            .Widget.ShouldBeOfType<WidgetWithThing>()
            .Thing.ShouldBeOfType<Thing>();
    }

    [Fact]
    public void resolve_singletons_via_constructor_with_object_dependency()
    {
        var thing = new Thing();

        var container = Container.For(_ =>
        {
            _.For<IWidget>().Use<WidgetWithThing>();
            _.For<IThing>().Use(thing);
            _.AddSingleton<GuyWithWidget>();
        });

        container.GetInstance<GuyWithWidget>()
            .Widget.ShouldBeOfType<WidgetWithThing>()
            .Thing.ShouldBe(thing);
    }

    [Fact]
    public void resolve_singletons_via_constructor_with_lambda_dependency()
    {
        var thing = new Thing();

        var container = Container.For(_ =>
        {
            _.For<IWidget>().Use<WidgetWithThing>();
            _.AddSingleton(new ThingFactory(thing));
            _.AddTransient(s => s.GetService<ThingFactory>().Build());
            _.AddSingleton<GuyWithWidget>();
        });

        container.GetInstance<GuyWithWidget>()
            .Widget.ShouldBeOfType<WidgetWithThing>()
            .Thing.ShouldBe(thing);
    }

    [Fact]
    public void can_resolve_with_closed_generic_depencency()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<AWidget>();
            _.For<IService<IWidget>>().Use<Service<IWidget>>();
            _.ForSingletonOf<GenericUsingGuy>().Use<GenericUsingGuy>();
        });

        container.GetInstance<GenericUsingGuy>()
            .WidgetService.Inner.ShouldBeOfType<AWidget>();
    }

    public class ThingFactory
    {
        private readonly IThing _thing;

        public ThingFactory(IThing thing)
        {
            _thing = thing;
        }

        public IThing Build()
        {
            return _thing;
        }
    }

    public interface IService<T>
    {
        T Inner { get; }
    }

    public class Service<T> : IService<T>
    {
        public Service(T inner)
        {
            Inner = inner;
        }

        public T Inner { get; }
    }

    public class GenericUsingGuy
    {
        public GenericUsingGuy(IService<IWidget> widgetService)
        {
            WidgetService = widgetService;
        }

        public IService<IWidget> WidgetService { get; }
    }

    public class GuyWithWidget
    {
        public GuyWithWidget(IWidget widget)
        {
            Widget = widget;
        }

        public IWidget Widget { get; }
    }

    public class WidgetWithThing : IWidget
    {
        public WidgetWithThing(IThing thing)
        {
            Thing = thing;
        }

        public IThing Thing { get; }

        public void DoSomething()
        {
        }
    }
}