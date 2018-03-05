using System.Composition.Hosting.Core;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class attribute_usage
    {
        [Fact]
        public void use_the_singleton_att()
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<SingleWidget>();
                
            });
            
            container.Model.For<IWidget>().Default.Lifetime
                .ShouldBe(ServiceLifetime.Singleton);
        }
        
        [Fact]
        public void use_the_scoped_att()
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<ScopedWidget>();
                
            });
            
            container.Model.For<IWidget>().Default.Lifetime
                .ShouldBe(ServiceLifetime.Scoped);
        }
        
        [Fact]
        public void instance_name()
        {
            var container = Container.For(_ =>
            {
                _.For<IWidget>().Use<OrangeWidget>();
                
            });
            
            container.Model.For<IWidget>().Default.Name.ShouldBe("Orange");
        }

        [InstanceName("Orange")]
        public class OrangeWidget : IWidget
        {
            public void DoSomething()
            {
                throw new System.NotImplementedException();
            }
        }
        
        [Singleton]
        public class SingleWidget : IWidget
        {
            public void DoSomething()
            {
                throw new System.NotImplementedException();
            }
        }

        [Scoped]
        public class ScopedWidget : IWidget
        {
            public void DoSomething()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}