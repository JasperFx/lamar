using System.Linq;
using System.Threading.Tasks;
using JasperFx.Core;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget5;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class type_scanning
{
    // Brought over from StructureMap

    [Fact]
    public void open_generic_scanning()
    {
        var container = new Container(i => i.Scan(s =>
        {
            s.AssemblyContainingType<type_scanning>();
            //s.WithDefaultConventions();
            s.AddAllTypesOf(typeof(ISomeInterface<>));
        }));

        container.GetInstance<ISomeInterface<Base>>()
            .ShouldNotBeNull();

        container.GetInstance<ISomeInterface<Derived>>()
            .ShouldBeOfType<Foo>()
            .ShouldNotBeNull();
    }

    [Fact]
    public async Task open_generic_scanning_async()
    {
        var container = await Container.BuildAsync(i => i.Scan(s =>
        {
            s.AssemblyContainingType<type_scanning>();
            //s.WithDefaultConventions();
            s.AddAllTypesOf(typeof(ISomeInterface<>));
        }));

        container.GetInstance<ISomeInterface<Base>>()
            .ShouldNotBeNull();

        container.GetInstance<ISomeInterface<Derived>>()
            .ShouldBeOfType<Foo>()
            .ShouldNotBeNull();
    }


    [Fact]
    public void Scanner_apply_should_only_register_two_instances()
    {
        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.ConnectImplementationsToTypesClosing(typeof(ISomeServiceOf<>));
            });
        });

        container.GetAllInstances<ISomeServiceOf<string>>().OrderBy(x => x.GetType().Name).Select(x => x.GetType())
            .ShouldHaveTheSameElementsAs(typeof(SomeService1), typeof(SomeService2));
    }

    [Fact]
    public void exclude_type_does_indeed_work()
    {
        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<IFoo>();
                x.ExcludeType<Foo2>();
            });
        });

        container.GetAllInstances<IFoo>()
            .Select(x => x.GetType())
            .ShouldHaveTheSameElementsAs(typeof(Foo1), typeof(Foo3));
    }

    //public class GenericClass3 : IGeneric<ConcreteChild> { }

    [Fact]
    public void StructureMap_Resolves_Generic_Child_Classes()
    {
        typeof(IGeneric<ConcreteChild>).IsAssignableFrom(typeof(GenericClass1)).ShouldBeTrue();
        typeof(IGeneric<ConcreteChild>).IsAssignableFrom(typeof(GenericClass2)).ShouldBeTrue();
        //Assert.ShouldBeTrue(typeof(IGeneric<ConcreteChild>).IsAssignableFrom(typeof(GenericClass3)));

        var container = new Container(cfg =>
        {
            cfg.Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.ConnectImplementationsToTypesClosing(typeof(IGeneric<>));
            });
        });

        var instances = container.GetAllInstances<IGeneric<ConcreteChild>>();

        instances.Any(t => t.GetType() == typeof(GenericClass1)).ShouldBeTrue();
        instances.Any(t => t.GetType() == typeof(GenericClass2)).ShouldBeTrue();
    }

    [Fact]
    public void be_smart_and_do_not_add_abstract_types()
    {
        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<ParentClass>();
            });
        });

        container.GetAllInstances<ParentClass>()
            .Single().ShouldBeOfType<ChildClass>();
    }

    [Fact]
    public void should_handle_default_closed_and_specific_closed()
    {
        var container = new Container(x =>
        {
            x.Scan(y =>
            {
                y.TheCallingAssembly();
                y.ConnectImplementationsToTypesClosing(typeof(IAmOpenGeneric<>));
            });

            x.For(typeof(IAmOpenGeneric<>)).Use(typeof(TheClosedGeneric<>));
        });

        container.GetInstance<IAmOpenGeneric<int>>().ShouldBeOfType<TheClosedGeneric<int>>();
        container.GetInstance<IAmOpenGeneric<string>>().ShouldBeOfType<SpecificClosedGeneric>();
    }

    [Fact]
    public void scanning_respects_order()
    {
        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<ICar>();
            });

            _.For<ICar>().Use<SUV>();
        });

        container.GetInstance<ICar>().ShouldBeOfType<SUV>();
    }

    [Fact]
    public void scanning_respects_order_2()
    {
        var container = new Container(_ =>
        {
            _.For<ICar>().Use<SUV>();

            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<ICar>();
            });
        });

        container.GetInstance<ICar>().ShouldNotBeOfType<SUV>();
    }

    [Fact]
    public async Task scanning_respects_order_async()
    {
        var container = await Container.BuildAsync(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<ICar>();
            });

            _.For<ICar>().Use<SUV>();
        });

        container.GetInstance<ICar>().ShouldBeOfType<SUV>();
    }

    [Fact]
    public async Task scanning_respects_order_2_async()
    {
        var container = await Container.BuildAsync(_ =>
        {
            _.For<ICar>().Use<SUV>();

            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.AddAllTypesOf<ICar>();
            });
        });

        container.GetInstance<ICar>().ShouldNotBeOfType<SUV>();
    }

    [Fact]
    public void has_the_correct_number_by_initialize()
    {
        var container = Container.For<BookRegistry>();
        container.GetAllInstances<IBook<SciFiBook>>().Count().ShouldBe(1);
    }

    [Fact]
    public void has_the_correct_number_by_configure()
    {
        var container = new Container(new BookRegistry());
        container.GetAllInstances<IBook<SciFiBook>>().Count().ShouldBe(1);
    }


    [Fact]
    public void fix_it()
    {
        var container = new Container(cfg =>
        {
            cfg.Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();

                scan.ConnectImplementationsToTypesClosing(typeof(IBird<>));
            });
        });

        container.GetAllInstances<IBird<Bird>>()
            .Select(x => x.GetType())
            .ShouldHaveTheSameElementsAs(typeof(BirdImpl), typeof(BirdBaseImpl));
    }

    [Fact]
    public void look_for_registries_fires_assembly_scanning_in_child()
    {
        var container = new Container(x =>
        {
            x.Scan(s =>
            {
                s.AssemblyContainingType<ITeam>();
                s.LookForRegistries();
            });
        });

        // From several registries in Widget5
        container.Model.HasRegistrationFor<IWidget>()
            .ShouldBeTrue();

        // Look for TeamRegistry
        container.Model.For<ITeam>().Instances.Select(x => x.ImplementationType.Name)
            .OrderBy(x => x)
            .ShouldHaveTheSameElementsAs("Broncos", "Chargers", "Chiefs", "Raiders");
    }

    [Fact]
    public async Task look_for_registries_fires_assembly_scanning_in_child_async()
    {
        var container = await Container.BuildAsync(x =>
        {
            x.Scan(s =>
            {
                s.AssemblyContainingType<ITeam>();
                s.LookForRegistries();
            });
        });

        // From several registries in Widget5
        container.Model.HasRegistrationFor<IWidget>()
            .ShouldBeTrue();

        // Look for TeamRegistry
        container.Model.For<ITeam>().Instances.Select(x => x.ImplementationType.Name)
            .OrderBy(x => x)
            .ShouldHaveTheSameElementsAs("Broncos", "Chargers", "Chiefs", "Raiders");
    }

    public interface ISomeInterface<in T>
    {
    }

    public class Base
    {
    }

    public class Derived : Base
    {
    }

    public class Foo : ISomeInterface<Base>
    {
    }

    public interface ISomeServiceOf<T>
    {
    }

    public class SomeService1 : ISomeServiceOf<string>
    {
    }

    public class SomeService2 : ISomeServiceOf<string>
    {
    }

    public interface IFoo
    {
    }

    public class Foo1 : IFoo
    {
    }

    public class Foo2 : IFoo
    {
    }

    public class Foo3 : IFoo
    {
    }


    public class Parent
    {
    }

    public class Child : Parent
    {
    }

    public class ConcreteChild : Child
    {
    }

    public interface IGeneric<in T> where T : class
    {
    }

    public class GenericClass1 : IGeneric<Parent>
    {
    }

    public class GenericClass2 : IGeneric<Child>
    {
    }


    public abstract class ParentClass
    {
    }

    public class ChildClass : ParentClass
    {
    }

    public abstract class OtherChildClass : ParentClass
    {
    }


    public interface ICar
    {
    }

    public class Sedan : ICar
    {
    }

    public class Coupe : ICar
    {
    }

    public class SUV : ICar
    {
    }

    [JasperFxIgnore]
    public class Crossover : ICar
    {
    }

    public interface IAmOpenGeneric<T>
    {
    }

    public class TheClosedGeneric<T> : IAmOpenGeneric<T>
    {
    }

    public class SpecificClosedGeneric : TheClosedGeneric<string>
    {
    }


    public interface INotificationHandler<in TNotification>
    {
        void Handle(TNotification notification);
    }

    public class BaseNotificationHandler : INotificationHandler<object>
    {
        public void Handle(object notification)
        {
        }
    }

    public class OpenNotificationHandler<TNotification> : INotificationHandler<TNotification>
    {
        public void Handle(TNotification notification)
        {
        }
    }

    public class Notification
    {
    }

    public class ConcreteNotificationHandler : INotificationHandler<Notification>
    {
        public void Handle(Notification notification)
        {
        }
    }
}

public class BookRegistry : ServiceRegistry
{
    public BookRegistry()
    {
        Scan(x =>
        {
            x.Exclude(type => type == typeof(DustCover<>));
            x.TheCallingAssembly();
            x.ConnectImplementationsToTypesClosing(typeof(IBook<>));
        });
    }
}

public class DustCover<T> : IBook<T>
{
    public DustCover(IBook<T> book)
    {
        Book = book;
    }

    public IBook<T> Book { get; }
}

public interface IBook<T>
{
}

public class SciFi
{
}

public class SciFiBook : IBook<SciFiBook>
{
}

public class Fantasy
{
}

public class FantasyBook : IBook<Fantasy>
{
}

public interface IBird<in T>
{
}

public class BirdBase
{
}

public class Bird : BirdBase
{
}

public class BirdImpl : IBird<Bird>
{
}

public class BirdBaseImpl : IBird<BirdBase>
{
}