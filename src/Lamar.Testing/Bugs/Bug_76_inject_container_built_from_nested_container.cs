using Baseline;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_76_inject_container_built_from_nested_container
    {
        public class GuyWhoUsesContainer
        {
            public IContainer Container { get; }

            public GuyWhoUsesContainer(IContainer container)
            {
                Container = container;
            }
            
            
        }

        [Fact]
        public void does_not_blow_up_idiomatic_lamar()
        {
            var container = Container.Empty();

            var nested = container.GetNestedContainer();

            var guy = nested.GetInstance<GuyWhoUsesContainer>();
            
            guy.Container.ShouldBeSameAs(nested);
        }
        
        [Fact]
        public void does_not_blow_up_aspnetcore_usage()
        {
            var container = Container.For(_ => _.AddTransient<GuyWhoUsesContainer>());

            var factory = container.GetInstance<IServiceScopeFactory>();

            var scope = factory.CreateScope();


            var guy = scope.ServiceProvider.GetService<GuyWhoUsesContainer>();
            
            guy.Container.ShouldBeSameAs(scope.ServiceProvider);
        }
    }
}