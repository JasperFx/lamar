using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class building_internal_types
{
    [Fact]
    public void still_choose_greediest_constructor()
    {
        var container = new Container(_ =>
        {
            _.AddTransient<IWidget, AWidget>();
            _.AddTransient<IService, WhateverService>();

            _.AddTransient<IGadget, PrivateGadget>();
            _.AddTransient<PrivateGadgetHolder>();
        });

        var gadget = container.GetInstance<IGadget>()
            .ShouldBeOfType<PrivateGadget>();

        gadget
            .Widget.ShouldBeOfType<AWidget>();

        gadget.Service.ShouldBeOfType<WhateverService>();

        gadget.Clock.ShouldBeNull();


        container.GetInstance<PrivateGadgetHolder>()
            .ShouldNotBeNull();
    }

    [Fact]
    public void can_build_public_nested_in_internal()
    {
        var container = new Container(_ => { _.For<Outer.Inner.InnerMost>().Use<Outer.Inner.InnerMost>(); });

        Assert.NotNull(container.GetInstance<Outer.Inner.InnerMost>());
    }
}

public class Outer
{
    internal class Inner
    {
        public class InnerMost
        {
        }
    }
}

public interface IGadget
{
}

public class PrivateGadgetHolder
{
    public PrivateGadgetHolder(IGadget gadget)
    {
        Gadget = gadget;
    }

    public IGadget Gadget { get; }
}

internal class PrivateGadget : IGadget
{
    public PrivateGadget(IWidget widget)
    {
        Widget = widget;
    }

    public PrivateGadget(IWidget widget, IService service) : this(widget)
    {
        Service = service;
    }

    public PrivateGadget(IWidget widget, IService service, IClock clock)
        : this(widget, service)
    {
        Clock = clock;
    }

    public IClock Clock { get; }
    public IService Service { get; }
    public IWidget Widget { get; }
}