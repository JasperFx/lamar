using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Instances
{
    public class basic_instance_behavior
    {
        [Fact]
        public void hashcode_logic()
        {
            var instance1 = new ConstructorInstance(typeof(IWidget), typeof(AWidget), ServiceLifetime.Transient)
            {
                Name = "One"
            };
            
            var instance2 = new ConstructorInstance(typeof(IWidget), typeof(AWidget), ServiceLifetime.Transient)
            {
                Name = "Two"
            };
            
            var instance3 = new ConstructorInstance(typeof(AWidget), typeof(AWidget), ServiceLifetime.Transient)
            {
                Name = "One"
            };
            
            var instance4 = new ConstructorInstance(typeof(IWidget), typeof(AWidget), ServiceLifetime.Transient)
            {
                Name = "One"
            };
            
            instance1.GetHashCode().ShouldBe(instance4.GetHashCode());
            
            instance1.GetHashCode().ShouldNotBe(instance2.GetHashCode());
            instance1.GetHashCode().ShouldNotBe(instance3.GetHashCode());
            instance2.GetHashCode().ShouldNotBe(instance3.GetHashCode());
        }
        
        [Fact]
        public void for_returns_the_instance()
        {
            var @object = new ObjectInstance(typeof(string),"foo");
            Instance.For(ServiceDescriptor.Singleton(@object))
                .ShouldBeSameAs(@object);
        }

        [Fact]
        public void for_object()
        {
            var descriptor = ServiceDescriptor.Singleton(typeof(string), "foo");
            var instance = Instance.For(descriptor)
                .ShouldBeOfType<ObjectInstance>();
            
            instance.ServiceType.ShouldBe(descriptor.ServiceType);
            instance.Lifetime.ShouldBe(descriptor.Lifetime);
        }
        
        [Fact]
        public void for_concrete_type()
        {
            var descriptor = ServiceDescriptor.Scoped<IClock, DisposableClock>();
            var instance = Instance.For(descriptor)
                .ShouldBeOfType<ConstructorInstance>();
            
            instance.ImplementationType.ShouldBe(descriptor.ImplementationType);
            instance.ServiceType.ShouldBe(descriptor.ServiceType);
            instance.Lifetime.ShouldBe(descriptor.Lifetime);
        }
        
        [Fact]
        public void for_lambda()
        {
            var descriptor = ServiceDescriptor.Singleton<IClock>(s => new Clock());
            
            var instance = Instance.For(descriptor)
                .ShouldBeOfType<LambdaInstance>();
            
            instance.Factory.ShouldBe(descriptor.ImplementationFactory);
            instance.ServiceType.ShouldBe(descriptor.ServiceType);
            instance.Lifetime.ShouldBe(descriptor.Lifetime);
        }
        
        
    }
}