using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class singletons
    {
        [Fact]
        public void generated_instance_returns_every_time()
        {
            var container = Container.For(_ =>
            {
                _.AddSingleton<IWidget, AWidget>();
            });

            var original = container.GetInstance<IWidget>();
            container.GetInstance<IWidget>().ShouldBeSameAs(original);
            container.GetInstance<IWidget>().ShouldBeSameAs(original);
            container.GetInstance<IWidget>().ShouldBeSameAs(original);
            container.GetInstance<IWidget>().ShouldBeSameAs(original);
        }    
    }
}