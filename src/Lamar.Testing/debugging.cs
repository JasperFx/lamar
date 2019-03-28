using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing
{
    public class debugging
    {
        [Fact]
        public void singleton()
        {
            var container = Container.For(x => x.For<IWidget>().Use<AWidget>().Singleton());
            
            container.GetInstance<IWidget>()
                .ShouldBeSameAs(container.GetInstance<IWidget>());
        }

        [Fact]
        public void scoped()
        {
            var container = Container.For(x => x.For<IWidget>().Use<AWidget>().Scoped());

            var original = container.GetInstance<IWidget>();
            original
                .ShouldBeSameAs(container.GetInstance<IWidget>());

            var nested = container.GetNestedContainer();

            var fromNested1 = nested.GetInstance<IWidget>();
            var fromNested2 = nested.GetInstance<IWidget>();
            
            fromNested1.ShouldBeSameAs(fromNested2);
            
            fromNested1.ShouldNotBeSameAs(original);
        }
    }
}