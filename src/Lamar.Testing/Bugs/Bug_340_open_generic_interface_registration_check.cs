using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_340_open_generic_interface_registration_check
{
    class ClassA {}
    
    interface IGenericService<T> {}
    
    class ServiceA : IGenericService<ClassA> {}
    
    [Fact]
    public void Test_it()
    {
        var container = new Container(x =>
        {
            x.For<IGenericService<ClassA>>().Use<ServiceA>();
        });

        var x = container.GetInstance<IGenericService<ClassA>>();

        container.GetInstance<IGenericService<ClassA>>()
            .ShouldNotBeNull();
        
        container.Model.HasRegistrationFor(typeof(IGenericService<>))
            .ShouldBeTrue();
    }
}