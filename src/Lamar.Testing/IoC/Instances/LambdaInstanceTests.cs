using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Instances;

public class LambdaInstanceTests
{
    private readonly ServiceGraph theServices = ServiceGraph.Empty();

    [Fact]
    public void derive_the_default_name()
    {
        LambdaInstance.For(s => new Clock())
            .Name.ShouldBe(nameof(Clock));
    }

    [Fact]
    public void requires_service_provider()
    {
        LambdaInstance.For(s => new Clock())
            .RequiresServiceProvider(null).ShouldBeTrue();
    }


    [Fact]
    public void build_a_variable_returns_a_get_instance_frame_when_scoped()
    {
        var instance = LambdaInstance.For<IClock>(s => new Clock(), ServiceLifetime.Scoped);
        instance.CreateVariable(BuildMode.Inline, null, false)
            .Creator.ShouldBeOfType<GetInstanceFrame>();
    }

    [Fact]
    public void build_a_variable_returns_an_injected_service_field_when_a_singleton_and_not_build()
    {
        var instance = LambdaInstance.For<IClock>(s => new Clock(), ServiceLifetime.Singleton);
        instance.CreateVariable(BuildMode.Dependency, null, false)
            .ShouldBeOfType<InjectedServiceField>();
    }
}