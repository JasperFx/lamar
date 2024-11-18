using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;


public class IKeyedServiceProvider_compliance
{
    #if NET8_0_OR_GREATER

    [Fact]
    public void register_by_name_using_dot_net_core_syntax()
    {
        var container = Container.For(services =>
        {
            
            services.AddKeyedSingleton<IWidget, AWidget>("one");
            services.AddKeyedScoped<IWidget>("two", (_, _) => new BWidget());
            services.AddKeyedSingleton<IWidget>("three", new CWidget());

            services.AddKeyedSingleton<CWidget>("C1");
            services.AddKeyedSingleton<CWidget>("C2");
            services.AddKeyedSingleton<CWidget>("C3");
        });

        container.GetInstance<IWidget>("one").ShouldBeOfType<AWidget>();
        container.GetKeyedService<IWidget>("one").ShouldBeOfType<AWidget>();
        
        container.GetInstance<IWidget>("two").ShouldBeOfType<BWidget>();
        container.GetKeyedService<IWidget>("two").ShouldBeOfType<BWidget>();
        
        container.GetInstance<IWidget>("three").ShouldBeOfType<CWidget>();
        container.GetKeyedService<IWidget>("three").ShouldBeOfType<CWidget>();
        
        container.GetInstance<CWidget>("C1").ShouldBeOfType<CWidget>();
        container.GetKeyedService<CWidget>("C2").ShouldBeOfType<CWidget>();
        
        container.GetKeyedService<CWidget>("C2")
            .ShouldBeSameAs(container.GetKeyedService<CWidget>("C2"));
        
        container.GetKeyedService<CWidget>("C2")
            .ShouldNotBeSameAs(container.GetKeyedService<CWidget>("C3"));
    }

    #endif
}
