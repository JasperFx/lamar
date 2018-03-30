using System;
using Lamar.IoC.Instances;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class try_get_instance
    {
        [Fact]
        public void complete_miss()
        {
            var container = Container.Empty();
            container.TryGetInstance<IFancy>()
                .ShouldBeNull();
        }

        [Fact]
        public void force_the_cached_miss_behavior()
        {
            var container = Container.Empty();
            container.TryGetInstance<IFancy>().ShouldBeNull();
            container.TryGetInstance<IFancy>().ShouldBeNull();
            container.TryGetInstance<IFancy>().ShouldBeNull();
            container.TryGetInstance<IFancy>().ShouldBeNull();
            container.TryGetInstance<IFancy>().ShouldBeNull();
        }

        [Fact]
        public void miss_then_hit_after_configure_with_policy_change()
        {
            var container = Container.Empty();
            container.TryGetInstance<IFancy>().ShouldBeNull();

            var container2 = Container.For(_ => _.Policies.OnMissingFamily<FancyFamily>());

            container2.TryGetInstance<IFancy>()
                .ShouldBeOfType<Very>();
        }

        [Fact]
        public void miss_then_hit_after_configure_adds_it()
        {
            var container = Container.Empty();
            container.TryGetInstance<IFancy>().ShouldBeNull();

            var container2 = Container.For(_ =>
            {
                _.For<IFancy>().Use(new NotReally());
            });
            
            container2.TryGetInstance<IFancy>()
                .ShouldBeOfType<NotReally>();
        }
        
        [Fact]
        public void try_get_by_name()
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Add<AWidget>().Named("Blue");
                _.For<IWidget>().Add<AWidget>().Named("Green");
                _.For<IWidget>().Add<AWidget>().Named("Red");
            });

            container.TryGetInstance<IWidget>("Blue").ShouldNotBeNull();
            container.TryGetInstance<IWidget>("Purple").ShouldBeNull();
        }
    }

    public interface IFancy
    {
    }

    public class Very : IFancy { }

    public class NotReally : IFancy { }

    public class FancyFamily : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type != typeof(IFancy)) return null;

            return new ServiceFamily(type, new IDecoratorPolicy[0], ConstructorInstance.For<IFancy, Very>());
        }
    }
}