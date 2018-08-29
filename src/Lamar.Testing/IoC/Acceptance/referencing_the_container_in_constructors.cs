using System;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class referencing_the_container_in_constructors
    {
        [Fact]
        public void get_context_in_singleton()
        {
            var container = Container.For(_ => { _.ForConcreteType<ContextUsingThing>().Configure.Singleton(); });
            
            container.GetInstance<ContextUsingThing>()
                .Services.ShouldBeSameAs(container);
        }
        
        [Fact]
        public void get_context_in_transient_from_parent()
        {
            var container = Container.For(_ =>
            {
                _.ForConcreteType<ContextUsingThing>().Configure.Transient();
            });
            
            container.GetInstance<ContextUsingThing>()
                .Services.ShouldBeSameAs(container);
        }
        
        [Fact]
        public void get_context_in_scoped_from_parent()
        {
            var container = Container.For(_ =>
            {
                _.ForConcreteType<ContextUsingThing>().Configure.Scoped();
            });
            
            container.GetInstance<ContextUsingThing>()
                .Services.ShouldBeSameAs(container);
        }
        
        [Fact]
        public void get_context_in_singleton_from_nested()
        {
            var container = Container.For(_ =>
            {
                _.ForConcreteType<ContextUsingThing>().Configure.Singleton();
            });
            var nested = container.GetNestedContainer();
            
            nested.GetInstance<ContextUsingThing>()
                .Services.ShouldBeSameAs(container);
        }
        
        [Fact]
        public void get_context_in_transient_from_nested()
        {
            var container = Container.For(_ =>
            {
                _.ForConcreteType<ContextUsingThing>().Configure.Transient();
            });
            var nested = container.GetNestedContainer();
            
            nested.GetInstance<ContextUsingThing>()
                .Services.ShouldBeSameAs(nested);
        }
        
        [Fact]
        public void get_context_in_scoped_from_nested()
        {
            var container = Container.For(_ =>
            {
                _.ForConcreteType<ContextUsingThing>().Configure.Scoped();
            });
            var nested = container.GetNestedContainer();
            
            nested.GetInstance<ContextUsingThing>()
                .Services.ShouldBeSameAs(nested);
        }

        [Fact]
        public void try_to_inject_container()
        {
            var container = Container.Empty();

            // Retrieving the IContainer always gets the root container
            container.GetInstance<IContainer>().ShouldBeSameAs(container);
            container.GetInstance<ContainerUsingThing>()
                .Container.ShouldBeSameAs(container);

            var nested = container.GetNestedContainer();
            nested.GetInstance<IContainer>().ShouldBeSameAs(nested);
            
            nested.GetInstance<ContainerUsingThing>()
                .Container.ShouldBeSameAs(nested);
        }
        

    }

    public class ContextUsingThing
    {
        public IServiceContext Services { get; }

        public ContextUsingThing(IServiceContext services)
        {
            Services = services;
        }
    }

    public class ContainerUsingThing
    {
        public IContainer Container { get; }

        public ContainerUsingThing(IContainer container)
        {
            Container = container;
        }
    }
}