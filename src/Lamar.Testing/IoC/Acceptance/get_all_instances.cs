using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class SimpleScenarios
    {
        public interface IFoo
        {
        }

        public class AFoo : IFoo
        {
        }

        public class BFoo : IFoo
        {
        }

        public class CFoo : IFoo
        {
        }

        [Fact]
        public void builds_all_instances_from_get_all()
        {
            var container = new Container(x =>
            {
                x.AddTransient<IFoo, AFoo>();
                x.AddTransient<IFoo, BFoo>();
                x.AddTransient<IFoo, CFoo>();
            });

            container.GetAllInstances<IFoo>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AFoo), typeof(BFoo), typeof(CFoo));
        }
        
        
        
        // SAMPLE: GetInstance
        [Fact]
        public void get_the_default_instance()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<AWidget>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<AWidget>();

            // or

            container.GetInstance(typeof(IWidget))
                .ShouldBeOfType<AWidget>();
        }

        // ENDSAMPLE

        // SAMPLE: GetInstance-by-name
        [Fact]
        public void get_a_named_instance()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<AWidget>().Named("A");
                x.For<IWidget>().Add<BWidget>().Named("B");
                x.For<IWidget>().Add<CWidget>().Named("C");
            });

            container.GetInstance<IWidget>("A").ShouldBeOfType<AWidget>();
            container.GetInstance<IWidget>("B").ShouldBeOfType<BWidget>();
            container.GetInstance<IWidget>("C").ShouldBeOfType<CWidget>();

            // or

            container.GetInstance(typeof(IWidget), "A").ShouldBeOfType<AWidget>();
            container.GetInstance(typeof(IWidget), "B").ShouldBeOfType<BWidget>();
            container.GetInstance(typeof(IWidget), "C").ShouldBeOfType<CWidget>();
        }

        // ENDSAMPLE

        // SAMPLE: get-all-instances
        [Fact]
        public void get_all_instances()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<AWidget>().Named("A");
                x.For<IWidget>().Add<BWidget>().Named("B");
                x.For<IWidget>().Add<CWidget>().Named("C");
            });
            
            container.QuickBuildAll<IWidget>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

            container.GetAllInstances<IWidget>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

            // or

            container.GetAllInstances(typeof(IWidget))
                .OfType<IWidget>() // returns an IEnumerable, so I'm casting here
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
        }

        // ENDSAMPLE

        public interface IWidget
        {
        }

        public class AWidget : IWidget
        {
        }

        public class BWidget : IWidget
        {
        }

        public class CWidget : IWidget
        {
        }
    }
}