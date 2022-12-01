using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public interface INotRegisteredParameter
{
}

public interface ISomeClass
{
}

public class SomeClass : ISomeClass
{
    //This returns expected:  Lamar.IoC.LamarException  Cannot build registered instance
    //public SomeClass(INotRegisteredParameter parameter){}
    //Null Ref
    public SomeClass(INotRegisteredParameter parameter = null)
    {
    }
}

public class Bug_46_optional_parameter
{
    [Fact]
    public void troubleshoot_constructor_OptionalArg_NullRef()
    {
        var container = new Container(services => { services.AddSingleton<ISomeClass, SomeClass>(); });

        var instance = container.GetInstance<ISomeClass>();
        instance.ShouldNotBeNull();
    }
}