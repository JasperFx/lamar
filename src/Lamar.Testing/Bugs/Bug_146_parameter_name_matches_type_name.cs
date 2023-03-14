using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_146_parameter_name_matches_type_name
{
    [Fact]
    public void UseCorrectTypes()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
            _.For<IWidget>().Use<Widget>();
        });

        var instance = container.GetInstance<IWidget>();

        instance.ShouldBeOfType<WidgetDecorator>();

        var widgetDecorator = (WidgetDecorator)instance;

        widgetDecorator.Widget.ShouldBeOfType<Widget>();
    }

    public interface IWidget
    {
        void DoStuff();
    }

    public class WidgetDecorator : IWidget
    {
        public WidgetDecorator(IWidget widget)
        {
            Widget = widget;
        }

        public IWidget Widget { get; }

        public void DoStuff()
        {
        }
    }

    public class Widget : IWidget
    {
        public void DoStuff()
        {
        }
    }
}