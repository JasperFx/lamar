using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_91_hyphens_in_instance_names
{
    [Fact]
    public void does_not_blow_up()
    {
        var container = Container.For(_ =>
        {
            _.For<IWidget>().Use<AWidget>().Named("a-widget");
            _.For<IWidget>().Use<MoneyWidget>().Named("money-widget");
        });

        container.GetInstance<IWidget>("a-widget").ShouldBeOfType<AWidget>();
        container.GetInstance<IWidget>("money-widget").ShouldBeOfType<MoneyWidget>();
    }
}