using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class resolve_by_name
{
    [Fact]
    public void get_the_right_instance_by_name()
    {
        var container = Container.For(_ =>
        {
            _.For<IWidget>().Add<BlueWidget>().Named("Blue");
            _.For<IWidget>().Add<GreenWidget>().Named("Green");
            _.For<IWidget>().Add<RedWidget>().Named("Red");
        });

        container.GetInstance<IWidget>("Blue").ShouldBeOfType<BlueWidget>();
        container.GetInstance<IWidget>("Green").ShouldBeOfType<GreenWidget>();
        container.GetInstance<IWidget>("Red").ShouldBeOfType<RedWidget>();
    }

    [Fact]
    public void scoping_by_name()
    {
        var container = Container.For(_ =>
        {
            _.For<IWidget>().Add<BlueWidget>().Named("Blue").Scoped();
            _.For<IWidget>().Add<GreenWidget>().Named("Green").Singleton();
            _.For<IWidget>().Add<RedWidget>().Named("Red");
        });

        container.GetInstance<IWidget>("Green")
            .ShouldBeSameAs(container.GetInstance<IWidget>("Green"));

        var parentBlue = container.GetInstance<IWidget>("Blue");
        container.GetInstance<IWidget>("Blue")
            .ShouldBeSameAs(parentBlue);

        using (var child = container.GetNestedContainer())
        {
            var childBlue = child.GetInstance<IWidget>("Blue");

            // should get the same
            child.GetInstance<IWidget>("Blue")
                .ShouldBeSameAs(childBlue);

            childBlue.ShouldNotBeSameAs(parentBlue);

            child.GetInstance<IWidget>("Green")
                .ShouldBeSameAs(container.GetInstance<IWidget>("Green"));
        }
    }

    public class BlueWidget : IWidget
    {
        public void DoSomething()
        {
        }
    }

    public class RedWidget : IWidget
    {
        public void DoSomething()
        {
        }
    }

    public class GreenWidget : IWidget
    {
        public void DoSomething()
        {
        }
    }
}