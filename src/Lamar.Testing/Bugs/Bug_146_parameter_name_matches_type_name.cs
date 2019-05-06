using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_146_parameter_name_matches_type_name
    {
        public interface IWidget
        {
            void DoStuff();
        }
        
        public class WidgetDecorator : IWidget
        {
            public IWidget Widget { get; }

            public WidgetDecorator(IWidget widget)
            {
                Widget = widget;
            }
        
            public void DoStuff() { }
        }
        
        public class Widget : IWidget
        {
            public void DoStuff() { }
        }
        
        [Fact]
        public void UseCorrectTypes()
        {
            Container container = new Container(_ =>
            {
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IWidget>().Use<Widget>();
            });

            IWidget instance = container.GetInstance<IWidget>();

            instance.ShouldBeOfType<WidgetDecorator>();
            
            WidgetDecorator widgetDecorator = (WidgetDecorator)instance;

            widgetDecorator.Widget.ShouldBeOfType<Widget>();
        }
    }
}