using System;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC;

public class NamedAttributeOnParameterTest
{
    [Fact]
    public void honor_named_attribute_on_parameter()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<GreenWidget>().Named("green");
            _.For<IWidget>().Use<BlueWidget>().Named("blue");
        });

        container.GetInstance<IWidget>().ShouldBeOfType<BlueWidget>();

        container.GetInstance<NamedParameterWidgetUser>().Widget.ShouldBeOfType<GreenWidget>();
    }

    private class GreenWidget : IWidget
    {
        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }

    private class BlueWidget : IWidget
    {
        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }

    private class NamedParameterWidgetUser
    {
        public NamedParameterWidgetUser([Named("green")] IWidget widget)
        {
            Widget = widget;
        }

        public IWidget Widget { get; }
    }
}