using System.Linq;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class use_if_none
    {
        [Fact]
        public void add_the_registration_if_there_is_no_prior()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().UseIfNone<AWidget>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<AWidget>();
        }

        [Fact]
        public void do_nothing_if_a_prior_registration()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<BlueWidget>();
                x.For<IWidget>().UseIfNone<AWidget>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<BlueWidget>();
            
            container.Model.For<IWidget>().Instances.Count()
                .ShouldBe(1);
        }
        
        [Fact]
        public void add_the_registration_if_there_is_no_prior_with_actual_object()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().UseIfNone(new AWidget());
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<AWidget>();
        }

        [Fact]
        public void do_nothing_if_a_prior_registration_with_actual_object()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<BlueWidget>();
                x.For<IWidget>().UseIfNone(new AWidget());
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<BlueWidget>();
            
            container.Model.For<IWidget>().Instances.Count()
                .ShouldBe(1);
        }
    }
}