using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

#if NET9_0_OR_GREATER
public class IServiceProviderIsKeyedService_implementation
{
    [Fact]
    public void explicit_checks_of_non_concrete_types()
    {
        var container = Container.For(services => { 
            services.For<IClock>().Use<Clock>();
            services.AddKeyedSingleton<IClock, Clock>("IClock");
        });
        
        container.IsKeyedService(typeof(IClock), "IClock").ShouldBeTrue();
        container.IsKeyedService(typeof(IWidget), "IWidget").ShouldBeFalse();
    }

    [Fact]
    public void fix_for_368_auto_resolve_enumerable_concretes_and_generics()
    {
        var container = Container.For(services =>
        {
            services.Scan(s =>
            {
                s.AssemblyContainingType(GetType());
                s.WithDefaultConventions();
                s.AddAllTypesOf<IService>();
            });

            services.AddKeyedSingleton<IService, ServiceA>("ServiceA");
            services.AddKeyedSingleton<IService, ServiceB>("ServiceB");

            services.For(typeof(IGenericService1<>)).Use(typeof(GenericService1<>));
            services.AddKeyedSingleton<IEnumerable<IService>, List<ServiceA>>("IEnumerable<ServiceA>");
            services.AddKeyedSingleton<IEnumerable<IService>, List<ServiceB>>("IEnumerable<ServiceB>");
            services.AddKeyedSingleton(typeof(IGenericService1<>), "GenericService1<>", typeof(GenericService1<>));
        });
        
        // For a true result to happen a keyed service must have been registered with an exact service key match.
        container.IsKeyedService(typeof(IEnumerable<IService>), "IEnumerable<ServiceA>").ShouldBeTrue();
        container.IsKeyedService(typeof(IEnumerable<IService>), "IEnumerable<ServiceB>").ShouldBeTrue();
        container.IsKeyedService(typeof(IGenericService1<>), "GenericService1<>").ShouldBeTrue();

        container.IsKeyedService(typeof(ConcreteClass), "ConcreteClass").ShouldBeFalse();
        container.IsKeyedService(typeof(IGenericService1<IService>), "IGenericService1<IService>").ShouldBeFalse();
        container.IsKeyedService(typeof(IGenericService1<ConcreteClass>), "IGenericService1<ConcreteClass>").ShouldBeFalse();

        var helloA = container.GetKeyedService<IService>("ServiceA").SayHello();
        helloA.ShouldBe("Hi there");

        var helloB = container.GetKeyedService<IService>("ServiceB").SayHello();
        helloB.ShouldBe("Hi there from typed service ServiceB");
    }

    [Fact]
    public void fix_for_391_dont_resolve_collections_with_not_registered_type()
    {
        var containerWithNoRegistrations = Container.Empty();

        containerWithNoRegistrations.IsKeyedService(typeof(IEnumerable<ConcreteClass>),"IEnumerable<ConcreteClass>").ShouldBeFalse();
        containerWithNoRegistrations.IsKeyedService(typeof(IReadOnlyCollection<ConcreteClass>) ,"IReadOnlyCollection<ConcreteClass>").ShouldBeFalse();
        containerWithNoRegistrations.IsKeyedService(typeof(IList<ConcreteClass>), "IList<ConcreteClass>").ShouldBeFalse();
        containerWithNoRegistrations.IsKeyedService(typeof(List<ConcreteClass>), "List<ConcreteClass>").ShouldBeFalse();

        var containerWithRegistrations = Container.For(services =>
                                                      {
                                                          services.ForConcreteType<ConcreteClass>();
                                                          services.AddKeyedSingleton<IEnumerable<IService>, List<ServiceA>>("IEnumerable<ServiceA>");
                                                      });
        
        // For a true result to happen a keyed service must have been registered with an exact service key match.
        containerWithRegistrations.IsKeyedService(typeof(IEnumerable<IService>), "IEnumerable<ServiceA>").ShouldBeTrue();

        containerWithRegistrations.IsKeyedService(typeof(IEnumerable<ConcreteClass>), "IEnumerable<ConcreteClass>").ShouldBeFalse();
        containerWithRegistrations.IsKeyedService(typeof(IReadOnlyCollection<ConcreteClass>), "IReadOnlyCollection<ConcreteClass>").ShouldBeFalse();
        containerWithRegistrations.IsKeyedService(typeof(IList<ConcreteClass>), "IList<ConcreteClass>").ShouldBeFalse();
        containerWithRegistrations.IsKeyedService(typeof(List<ConcreteClass>), "List<ConcreteClass>").ShouldBeFalse();
    }

    public interface IService
    {
        string SayHello();
    }

    public class ServiceA : IService
    {
        public string SayHello() => "Hi there";
    }

    public class ServiceB : IService
    {
        private readonly string _serviceKey;

        public ServiceB([ServiceKey]string serviceKey)
        {
            _serviceKey = serviceKey;
        }

        public string SayHello() => $"Hi there from typed service {_serviceKey}";
    }

    public class ConcreteClass
    {
    }

    public interface IGenericService1<T>
    {
    }

    public class GenericService1<T> : IGenericService1<T>
    {
    }
}
#endif