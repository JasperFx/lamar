using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Instances;

public class ObjectInstanceTests
{
    [Fact]
    public void derive_the_default_name()
    {
        ObjectInstance.For(new Clock())
            .Name.ShouldBe(nameof(Clock));
    }

    [Fact]
    public void build_a_resolver()
    {
        var clock = new Clock();
        var instance = ObjectInstance.For<IClock>(clock);


        instance.Resolve(null).ShouldBeSameAs(clock);
    }

    [Theory]
    [InlineData(BuildMode.Dependency)]
    [InlineData(BuildMode.Inline)]
    [InlineData(BuildMode.Build)]
    public void service_variable_is_injected_service_Variable(BuildMode mode)
    {
        var clock = new Clock();
        var instance = ObjectInstance.For<IClock>(clock);

        instance.CreateVariable(mode, null, false).ShouldBeOfType<InjectedServiceField>().Instance.ShouldBe(instance);
    }
}