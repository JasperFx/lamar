using System;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class NamedAttributeOnParameterTest
    {
        class GreenWidget : IWidget
        {
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }

        class BlueWidget : IWidget
        {
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }

        class NamedParameterWidgetUser
        {
            public IWidget Widget { get; }

            public NamedParameterWidgetUser([Named("green")]IWidget widget)
            {
                Widget = widget;
            }
        }
        
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
    }
}