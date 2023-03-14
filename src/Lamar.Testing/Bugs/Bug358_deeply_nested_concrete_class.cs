using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug358_deeply_nested_concrete_class
{
    // passes
    [Fact]
    public void resolves_nested1()
    {
        var container = new Container(_ => { });
        var actual = container.GetInstance<Nested1>();
        Assert.NotNull(actual);
    }

    // fails
    [Fact]
    public void resolves_nested2()
    {
        var container = new Container(_ => { });
        var actual = container.GetInstance<Nested1.Nested2>();

        /*
            Lamar.IoC.LamarMissingRegistrationException
            No service registrations exist or can be derived for Throwaway.LamarConcreteTypeResolutionAssumptions.Nested1.Nested2
               at Lamar.IoC.Scope.GetInstance(Type serviceType)
               at Lamar.IoC.Scope.GetInstance[T]()
               at Throwaway.LamarConcreteTypeResolutionAssumptions.resolves_nested2() 
         */

        Assert.NotNull(actual);
    }

    public class Nested1
    {
        public class Nested2
        {
        }
    }
}