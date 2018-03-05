using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class explaining_why_constructor_cannot_be_filled
    {
        [Fact]
        public void has_simple_argument()
        {
            var graph = ServiceGraph.For(x => x.AddTransient<IWidget, AWidget>());

            var ctor = ConstructorInstance.For<WithSimples>().DetermineConstructor(graph, out var message);

            message.ShouldContain("* int number is a 'simple' type that cannot be auto-filled");
        }

        [Fact]
        public void has_unknown_dependency()
        {
            var graph = ServiceGraph.For(x => x.AddTransient<IWidget, AWidget>());

            var ctor = ConstructorInstance.For<WithHitsAndMisses>().DetermineConstructor(graph, out var message);

            message.ShouldContain("* int number is a 'simple' type that cannot be auto-filled");
            message.ShouldContain(
                "* Rule is not registered within this container and cannot be auto discovered by any missing family policy");
        }
    }

    public class WithHitsAndMisses
    {
        public WithHitsAndMisses(int number, IWidget widget)
        {
        }

        public WithHitsAndMisses(IWidget widget, Rule rule)
        {
        }
    }

    public class WithSimples
    {
        public WithSimples(int number, IWidget widget)
        {
        }
    }
}