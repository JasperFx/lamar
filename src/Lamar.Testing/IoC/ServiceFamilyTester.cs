using System.Linq;
using JasperFx.Reflection;
using Lamar.IoC.Instances;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC;

public class ServiceFamilyTester
{
    [Fact]
    public void the_last_instance_is_the_default()
    {
        var family = new ServiceFamily(typeof(IWidget), new IDecoratorPolicy[0],
            ConstructorInstance.For<IWidget, AWidget>(), ConstructorInstance.For<IWidget, ColorWidget>());

        family.Default.As<ConstructorInstance>().ImplementationType.ShouldBe(typeof(ColorWidget));
    }

    [Fact]
    public void make_all_the_names_unique()
    {
        var family = new ServiceFamily(typeof(IWidget), new IDecoratorPolicy[0],
            ConstructorInstance.For<IWidget, AWidget>(), ConstructorInstance.For<IWidget, AWidget>(),
            ConstructorInstance.For<IWidget, AWidget>(), ConstructorInstance.For<IWidget, ColorWidget>(),
            ConstructorInstance.For<IWidget, ColorWidget>(), ConstructorInstance.For<IWidget, MoneyWidget>());

        family.Instances.ContainsKey("aWidget1").ShouldBeTrue();
        family.Instances.ContainsKey("aWidget2").ShouldBeTrue();
        family.Instances.ContainsKey("aWidget3").ShouldBeTrue();
        family.Instances.ContainsKey("colorWidget1").ShouldBeTrue();
        family.Instances.ContainsKey("colorWidget2").ShouldBeTrue();
        family.Instances.ContainsKey("moneyWidget").ShouldBeTrue();
    }

    [Fact]
    public void stores_all_in_order()
    {
        var allInstances = new Instance[]
        {
            ConstructorInstance.For<IWidget, AWidget>(),
            ConstructorInstance.For<IWidget, AWidget>(),
            ConstructorInstance.For<IWidget, ColorWidget>(),
            ConstructorInstance.For<IWidget, MoneyWidget>()
        };
        var family = new ServiceFamily(typeof(IWidget), new IDecoratorPolicy[0], allInstances);

        family.All.ShouldBe(allInstances);
    }

    [Fact]
    public void setting_the_is_default_property_on_instance()
    {
        var family = new ServiceFamily(typeof(IWidget), new IDecoratorPolicy[0],
            ConstructorInstance.For<IWidget, AWidget>(), ConstructorInstance.For<IWidget, AWidget>(),
            ConstructorInstance.For<IWidget, AWidget>(), ConstructorInstance.For<IWidget, ColorWidget>(),
            ConstructorInstance.For<IWidget, ColorWidget>(), ConstructorInstance.For<IWidget, MoneyWidget>());

        family.Instances["moneyWidget"].IsDefault.ShouldBeTrue();

        foreach (var instance in family.Instances.Values.Where(x => x.Name != "moneyWidget"))
            instance.IsDefault.ShouldBeFalse();
    }
}