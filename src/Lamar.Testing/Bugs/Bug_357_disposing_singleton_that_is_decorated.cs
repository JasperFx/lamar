using System;
using Baseline;
using Lamar.IoC;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_357_disposing_singleton_that_is_decorated
{
    [Fact]
    public void outer_and_inner_is_only_disposed_by_root_container()
    {
        var container = Container.For(x =>
        {
            x.For<IThing>().Use<SingleThing>().Singleton();
            x.For<IThing>().DecorateAllWith<WrappedThing>();
        });

        var wrapped = container.GetInstance<IThing>().ShouldBeOfType<WrappedThing>();
        var inner = wrapped.Inner.ShouldBeOfType<SingleThing>();

        var nested1 = container.GetNestedContainer();
        var thing1 = nested1.GetInstance<IThing>();

        var nested2 = container.GetNestedContainer();
        var thing2 = nested2.GetInstance<IThing>();
        
        nested2.Dispose();
        
        wrapped.WasDisposed.ShouldBeFalse();
        inner.WasDisposed.ShouldBeFalse();
        
        nested1.Dispose();
        
        wrapped.WasDisposed.ShouldBeFalse();
        inner.WasDisposed.ShouldBeFalse();
        
        container.Dispose();
        
        wrapped.WasDisposed.ShouldBeTrue();
        inner.WasDisposed.ShouldBeTrue();
    }

}

public interface IThing
{
    
}

public class SingleThing : IThing, IDisposable
{
    public void Dispose()
    {
        WasDisposed = true;
    }

    public bool WasDisposed { get; set; }
}

public class WrappedThing : IThing, IDisposable
{
    public IThing Inner { get; }

    public WrappedThing(IThing inner)
    {
        Inner = inner;
    }
    
    public void Dispose()
    {
        WasDisposed = true;
    }

    public bool WasDisposed { get; set; }
}


    public class Bug_tbd_decorator_singleton_nested_nestedContainer_should_not_be_disposed
    {
        public interface ISingleton
        {
            bool Disposed { get; }
        }

        public class Singleton : ISingleton, IDisposable
        {
            public Guid Id { get; private set; } = Guid.NewGuid();
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        public class SingletonDecorator : ISingleton, IDisposable
        {
            public bool Disposed { get; private set; }
            public ISingleton Singleton;
            public SingletonDecorator(ISingleton singleton)
            {
                Singleton = singleton;
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        public interface IScopedServiceB
        {
            ISingleton Singleton { get; }
        }

        public class ScopedServiceB : IScopedServiceB
        {
            public ISingleton Singleton { get; private set; }
            public ScopedServiceB(ISingleton singleton)
            {
                Singleton = singleton;
            }
        }

        public interface IScopedServiceA
        {
            IScopedServiceB ScopedServiceB { get; }
        }

        public class ScopedServiceA : IScopedServiceA
        {
            public IScopedServiceB ScopedServiceB { get; private set; }
            public ScopedServiceA(IScopedServiceB scopedServiceB)
            {
                ScopedServiceB = scopedServiceB;
            }
        }

        [Fact]
        public void Decorator_singleton_should_not_be_disposed()
        {
            // Dependency chain: ScopedServiceA > ScopedServiceB > SingletonDecorator > Singleton

            var container = new Container(x =>
            {
                x.For<IScopedServiceA>().Use<ScopedServiceA>().Scoped();
                x.For<IScopedServiceB>().Use<ScopedServiceB>().Scoped();

                x.ForSingletonOf<ISingleton>().Use<Singleton>();
                x.For<ISingleton>().DecorateAllWith<SingletonDecorator>();
            });

            SingletonDecorator wrapped;
            ISingleton nested;
                


            IScopedServiceA serviceA;

            //var singleton = container.GetInstance<ISingleton>(); // When uncommented, Singleton Dispose is not called

            using (var nested1 = container.GetNestedContainer())
            {
                wrapped = container.GetInstance<ISingleton>().ShouldBeOfType<SingletonDecorator>();
                nested = wrapped.Singleton;
                
                //var singleton = nested1.GetInstance<ISingleton>(); // When uncommented, Singleton Dispose is not called

                var container2 = nested1.GetInstance<IContainer>();
                using (var nested2 = container2.GetNestedContainer())
                {
                    //var singleton = nested2.GetInstance<ISingleton>(); // When uncommented, Singleton Dispose is not called

                    serviceA = nested2.GetInstance<IScopedServiceA>(); // ScopedServiceA > ScopedServiceB > SingletonDecorator > Singleton, Singleton is Disposed when nested1 is Disposed.
                    //var serviceB = nested2.GetInstance<IScopedServiceB>(); // ScopedServiceB > SingletonDecorator > Singleton, Singleton Dispose is not called.
                }

                serviceA = nested1.GetInstance<IScopedServiceA>();
                
                wrapped.Disposed.ShouldBeFalse();
                nested.Disposed.ShouldBeFalse();



            } // *** Singleton Dispose() is called ***

            wrapped.Disposed.ShouldBeFalse();
            nested.Disposed.ShouldBeFalse();
            
            serviceA = container.GetInstance<IScopedServiceA>();
            serviceA.ScopedServiceB.Singleton.Disposed.ShouldBeFalse(); // True, not expected

            // Observations:
            // 1) If I comment out the decorator registration "x.For<ISingleton>().DecorateAllWith<SingletonDecorator>();" - Singleton is not disposed.
            // 2) If I resolve an instance of ISingleton at any line of code before resolving IScopedServiceA - Singleton is not disposed.
            // 3) If I get an instance of IScopedServiceB instead of IScopedServiceA in nested2 - Singleton is not disposed.
        }
    }
