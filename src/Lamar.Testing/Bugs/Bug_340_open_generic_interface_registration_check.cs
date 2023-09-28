using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_340_open_generic_interface_registration_check
{
    class ClassA {}
    
    interface IGenericService<T> {}
    
    class ServiceA : IGenericService<ClassA> {}
    
    [Fact]
    public void do_not_blow_up()
    {
        var container = new Container(x =>
        {
            x.For<IGenericService<ClassA>>().Use<ServiceA>();
        });

        var x = container.GetInstance<IGenericService<ClassA>>();

        container.GetInstance<IGenericService<ClassA>>()
            .ShouldNotBeNull();
        
        // This should return false. There's no registration for the 
        // open type, only a specific closed type
        container.Model.HasRegistrationFor(typeof(IGenericService<>))
            .ShouldBeFalse();
    }
}