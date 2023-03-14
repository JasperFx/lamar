using System;
using System.Diagnostics;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_324_scoping_within_on_creation
{
    [Fact]
    public void make_it_work()
    {
        var container = Container.For(x =>
        {
            x.For<IWidget>().Use<Widget>().Scoped();

            // This ignores the concrete FakeWrapper implementation.
            // It should return a RealWrapper containing the scoped widget from the container.
            // But it seems to create another Widget instance instead.
            x.For<IWidgetWrapper>().Use<FakeWrapper>()
                .OnCreation((context, concrete) => new RealWrapper(context.GetInstance<IWidget>()));
        });

        var widget = container.GetInstance<IWidgetWrapper>()
            .ShouldBeOfType<RealWrapper>().GetWidget();

        widget.ShouldBeSameAs(container.GetInstance<IWidget>());
    }

    public interface IWidget
    {
    }

    public class Widget : IWidget
    {
        public Widget()
        {
            Debug.WriteLine("Constructing a Widget");
        }
    }

    public interface IWidgetWrapper
    {
        IWidget GetWidget();
    }

    public class FakeWrapper : IWidgetWrapper
    {
        public IWidget GetWidget()
        {
            throw new NotImplementedException();
        }
    }

    public class RealWrapper : IWidgetWrapper
    {
        private readonly IWidget inner;

        public RealWrapper(IWidget inner)
        {
            this.inner = inner;
        }

        public IWidget GetWidget()
        {
            return inner;
        }
    }
}