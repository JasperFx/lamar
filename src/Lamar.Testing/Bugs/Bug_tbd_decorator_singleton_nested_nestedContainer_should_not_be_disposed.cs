using System;
using Xunit;

namespace Lamar.Testing.Bugs
{
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

        public class SingletonDecorator : ISingleton
        {
            public bool Disposed { get { return Singleton.Disposed; } }
            public ISingleton Singleton;
            public SingletonDecorator(ISingleton singleton)
            {
                Singleton = singleton;
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

            IScopedServiceA serviceA;
            bool isDisposed;

            //var singleton = container.GetInstance<ISingleton>(); // When uncommented, Singleton Dispose is not called

            using (var nested1 = container.GetNestedContainer())
            {
                //var singleton = nested1.GetInstance<ISingleton>(); // When uncommented, Singleton Dispose is not called

                var container2 = nested1.GetInstance<IContainer>();
                using (var nested2 = container2.GetNestedContainer())
                {
                    //var singleton = nested2.GetInstance<ISingleton>(); // When uncommented, Singleton Dispose is not called

                    serviceA = nested2.GetInstance<IScopedServiceA>(); // ScopedServiceA > ScopedServiceB > SingletonDecorator > Singleton, Singleton is Disposed when nested1 is Disposed.
                    //var serviceB = nested2.GetInstance<IScopedServiceB>(); // ScopedServiceB > SingletonDecorator > Singleton, Singleton Dispose is not called.
                }

                serviceA = nested1.GetInstance<IScopedServiceA>();
                isDisposed = serviceA.ScopedServiceB.Singleton.Disposed; // False as expected

                Assert.False(isDisposed);

            } // *** Singleton Dispose() is called ***

            serviceA = container.GetInstance<IScopedServiceA>();
            isDisposed = serviceA.ScopedServiceB.Singleton.Disposed; // True, not expected

            Assert.False(isDisposed);

            // Observations:
            // 1) If I comment out the decorator registration "x.For<ISingleton>().DecorateAllWith<SingletonDecorator>();" - Singleton is not disposed.
            // 2) If I resolve an instance of ISingleton at any line of code before resolving IScopedServiceA - Singleton is not disposed.
            // 3) If I get an instance of IScopedServiceB instead of IScopedServiceA in nested2 - Singleton is not disposed.
        }
    }
}
