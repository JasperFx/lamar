using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;
#if NET6_0_OR_GREATER
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

            services.For(typeof(IGenericService1<>)).Use(typeof(GenericService1<>));
            services.AddKeyedSingleton<IEnumerable<IService>, List<ServiceA>>("IEnumerable<ServiceA>");
            services.AddKeyedSingleton<IEnumerable<IService>, List<ServiceB>>("IEnumerable<ServiceB>");
            services.AddKeyedSingleton(typeof(IGenericService1<>), "GenericService1<>", typeof(GenericService1<>));

        });

        container.IsKeyedService(typeof(ConcreteClass), "ConcreteClass").ShouldBeFalse();
        container.IsKeyedService(typeof(IEnumerable<IService>), "IEnumerable<ServiceA>").ShouldBeTrue();
        container.IsKeyedService(typeof(IGenericService1<IService>), "IGenericService1<IService>").ShouldBeTrue();
        container.IsKeyedService(typeof(IGenericService1<ConcreteClass>), "IGenericService1<ConcreteClass>").ShouldBeTrue();
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

        containerWithRegistrations.IsKeyedService(typeof(IEnumerable<ConcreteClass>), "IEnumerable<ConcreteClass>").ShouldBeTrue();
        containerWithRegistrations.IsKeyedService(typeof(IReadOnlyCollection<ConcreteClass>), "IReadOnlyCollection<ConcreteClass>").ShouldBeTrue();
        containerWithRegistrations.IsKeyedService(typeof(IList<ConcreteClass>), "IList<ConcreteClass>").ShouldBeTrue();
        containerWithRegistrations.IsKeyedService(typeof(List<ConcreteClass>), "List<ConcreteClass>").ShouldBeTrue();
    }

    public interface IService
    {
    }

    public class ServiceA : IService
    {
    }

    public class ServiceB : IService
    {
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