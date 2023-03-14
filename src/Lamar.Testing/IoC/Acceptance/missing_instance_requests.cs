using JasperFx.CodeGeneration;
using Lamar.IoC;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class missing_instance_requests
{
    [Fact]
    public void descriptive_error_when_no_registration_exists()
    {
        var container = new Container(new ServiceRegistry());


        Exception<LamarMissingRegistrationException>.ShouldBeThrownBy(() => { container.GetInstance<IWidget>(); })
            .Message.ShouldContain(typeof(IWidget).FullNameInCode());
    }

    [Fact]
    public void descriptive_error_when_no_registration_exists_for_the_name()
    {
        var container = new Container(new ServiceRegistry());

        Exception<LamarMissingRegistrationException>.ShouldBeThrownBy(() => { container.GetInstance<IWidget>("Blue"); })
            .Message.ShouldBe("Unknown service registration 'Blue' of StructureMap.Testing.Widget.IWidget");
    }

    [Fact]
    public void descriptive_error_when_no_registration_exists_for_the_name_2()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<AWidget>().Named("Red");
            _.For<IWidget>().Use<AWidget>().Named("Green");
        });

        Exception<LamarMissingRegistrationException>.ShouldBeThrownBy(() => { container.GetInstance<IWidget>("Blue"); })
            .Message.ShouldBe("Unknown service registration 'Blue' of StructureMap.Testing.Widget.IWidget");
    }
}