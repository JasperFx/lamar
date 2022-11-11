using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Lamar.Testing.IoC.Acceptance;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_104_scoped_resolver_needs_to_be_hardened_for_scoped_resolution
    {
        [Fact]
        public void it_should_not_blow_up()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<AWidget>().Scoped().Named("A");
                x.For<IWidget>().Use<BWidget>().Scoped().Named("B");
                x.For<IWidget>().Use<CWidget>().Scoped().Named("C");
                
            });




            void tryToResolveAll()
            {
                container.GetInstance<IWidget>("A").ShouldNotBeNull();
                container.GetInstance<IWidget>("B").ShouldNotBeNull();
                container.GetInstance<IWidget>("C").ShouldNotBeNull();
            }

            var list = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                list.Add(Task.Factory.StartNew(tryToResolveAll));
            }

            Task.WaitAll(list.ToArray());
        }
    }
}