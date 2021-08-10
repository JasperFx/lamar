using Shouldly;
using System.Linq;
using Xunit;

namespace StructureMap.Testing.Configuration.DSL
{
    public class AddTypesTester
    {
        public interface IAddTypes
        {
        }

        public class RedAddTypes : IAddTypes
        {
        }

        public class GreenAddTypes : IAddTypes
        {
        }

        public class BlueAddTypes : IAddTypes
        {
        }

        public class PurpleAddTypes : IAddTypes
        {
        }

        #region sample_named-instances-shorthand
        [Fact]
        public void A_concrete_type_is_available_by_name_when_it_is_added_by_the_shorthand_mechanism()
        {
            IContainer container = new Container(r => r.For<IAddTypes>().AddInstances(x =>
            {
                x.Type<RedAddTypes>().Named("Red");
                x.Type<GreenAddTypes>().Named("Green");
                x.Type<BlueAddTypes>().Named("Blue");
                x.Type<PurpleAddTypes>();
            }));
            // retrieve the instances by name
            container.GetInstance<IAddTypes>("Red").IsType<RedAddTypes>();
            container.GetInstance<IAddTypes>("Green").IsType<GreenAddTypes>();
            container.GetInstance<IAddTypes>("Blue").IsType<BlueAddTypes>();
        }

        #endregion

        [Fact]
        public void A_concrete_type_is_available_when_it_is_added_by_the_shorthand_mechanism()
        {
            IContainer container = new Container(registry =>
            {
                registry.For<IAddTypes>().AddInstances(x =>
                {
                    x.Type<RedAddTypes>();
                    x.Type<GreenAddTypes>();
                    x.Type<BlueAddTypes>();
                    x.Type<PurpleAddTypes>();
                });
            });

            container.GetAllInstances<IAddTypes>().Count().ShouldBe(4);
        }

        [Fact]
        public void Make_sure_that_we_dont_double_dip_instances_when_we_register_a_type_with_a_name()
        {
            IContainer container = new Container(r =>
            {
                r.For<IAddTypes>().AddInstances(x =>
                {
                    x.Type<GreenAddTypes>();
                    x.Type<BlueAddTypes>();
                    x.Type<PurpleAddTypes>();
                    x.Type<PurpleAddTypes>().Named("Purple");
                });
            });

            container.GetAllInstances<IAddTypes>().Count().ShouldBe(4);
        }
    }
}