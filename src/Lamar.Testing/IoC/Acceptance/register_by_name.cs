using System.Linq;

using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class register_by_name
    {
        public class BlueWidget : IWidget
        {
            public void DoSomething()
            {
                
            }
        }
        
        public class RedWidget : IWidget
        {
            public void DoSomething()
            {
                
            }
        }
        
        public class GreenWidget : IWidget
        {
            public void DoSomething()
            {
                
            }
        }

        public class YellowWidget : IWidget
        {
            public void DoSomething()
            {
                
            }
        }

        public class OrangeWidget : IWidget
        {
            public void DoSomething()
            {
                
            }
        }

        [Fact]
        public void register_all_instances_by_name_should_set_isexplicitlynamed_to_true_for_all_instances()
        {
            var container = Container.For(_ =>
                                          {
                                              _.For<IWidget>().Add<BlueWidget>().Named("Blue");
                                              _.For<IWidget>().Add<GreenWidget>().Named("Green");
                                              _.For<IWidget>().Add(_ => new RedWidget()).Named("Red");
                                              _.For<IWidget>().Add(new OrangeWidget()).Named("Orange");
                                              _.For<YellowWidget>().Add<YellowWidget>().Named("Yellow");
                                          });

           container.Model.AllInstances
               .Where(instanceRef => instanceRef.ServiceType == typeof(IWidget) || instanceRef.ServiceType == typeof(YellowWidget))
               .ShouldAllBe(instanceRef => instanceRef.Instance.IsExplicitlyNamed == true);
        }

        [Fact]
        public void register_all_instances_not_by_name_should_set_isexplicitlynamed_to_false_for_all_instances()
        {
            var container = Container.For(_ =>
                                          {
                                              _.For<IWidget>().Add<BlueWidget>();
                                              _.For<IWidget>().Add<GreenWidget>();
                                              _.For<IWidget>().Add(_ => new RedWidget());
                                              _.For<IWidget>().Add(new OrangeWidget());
                                              _.For<YellowWidget>().Add<YellowWidget>();
                                          });

            container.Model.AllInstances
                .Where(instanceRef => instanceRef.ServiceType == typeof(IWidget) || instanceRef.ServiceType == typeof(YellowWidget))
                .ShouldAllBe(instanceRef => instanceRef.Instance.IsExplicitlyNamed == false);
        }

        [Fact]
        public void register_some_instances_by_name_and_some_instances_not_by_name_should_set_isexplicitlynamed_accordingly()
        {
            var container = Container.For(_ =>
                                          {
                                              _.For<IWidget>().Add<BlueWidget>().Named("Blue");
                                              _.For<IWidget>().Add<GreenWidget>();
                                              _.For<IWidget>().Add(_ => new RedWidget()).Named("Red");
                                              _.For<IWidget>().Add(new OrangeWidget()).Named("Orange");
                                              _.For<YellowWidget>().Add<YellowWidget>();
                                          });

            container.Model.AllInstances
                .Where(instanceRef => instanceRef.ImplementationType == typeof(BlueWidget) || instanceRef.ImplementationType == typeof(RedWidget) || instanceRef.ImplementationType == typeof(OrangeWidget))
                .ShouldAllBe(instanceRef => instanceRef.Instance.IsExplicitlyNamed == true);

            container.Model.AllInstances
                .Where(instanceRef => instanceRef.ImplementationType == typeof(GreenWidget) || instanceRef.ImplementationType == typeof(YellowWidget))
                .ShouldAllBe(instanceRef => instanceRef.Instance.IsExplicitlyNamed == false);
        }
    }
}