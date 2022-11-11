using System;
using System.Threading.Tasks;
using Baseline;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    
    public enum RegistrationType
    {
        Object,
        Lambda,
        Type
    }
    public class disposing_container
    {

        [Fact]
        public void default_lock_is_unlocked()
        {
            new Container(new ServiceRegistry()).DisposalLock.ShouldBe(DisposalLock.Unlocked);
        }

        [Fact]
        public void try_to_dispose_when_ignored_should_do_nothing()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();
            });

            container.DisposalLock = DisposalLock.Ignore;

            container.Dispose();
            container.Dispose();
            container.Dispose();

            container.Model.For<IWidget>().Default.ImplementationType.ShouldBe(typeof(AWidget));
        }
        
        [Fact]
        public async Task try_to_async_dispose_when_ignored_should_do_nothing()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();
            });

            container.DisposalLock = DisposalLock.Ignore;

            await container.DisposeAsync();
            await container.DisposeAsync();
            await container.DisposeAsync();

            container.Model.For<IWidget>().Default.ImplementationType.ShouldBe(typeof(AWidget));
        }

        [Fact]
        public void try_to_dispose_when_locked_should_throw_an_invalid_operation_exception()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();
            });

            container.DisposalLock = DisposalLock.ThrowOnDispose;

            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                container.Dispose();
            });

            container.Model.For<IWidget>().Default.ImplementationType.ShouldBe(typeof(AWidget));
        }
        
        [Fact]
        public async Task try_to_async_dispose_when_locked_should_throw_an_invalid_operation_exception()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();
            });

            container.DisposalLock = DisposalLock.ThrowOnDispose;

            await Exception<InvalidOperationException>.ShouldBeThrownBy(async () =>
            {
                await container.DisposeAsync();
            });

            container.Model.For<IWidget>().Default.ImplementationType.ShouldBe(typeof(AWidget));
        }

        [Fact]
        public void disposing_a_main_container_will_dispose_an_object_injected_into_the_container()
        {
            var disposable = new C2Yes();
            var container = new Container(x => x.For<C2Yes>().Use(disposable));

            container.Dispose();

            disposable.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task async_disposing_a_main_container_will_dispose_an_object_injected_into_the_container()
        {
            var disposable = new C2Yes();
            var container = new Container(x => x.For<C2Yes>().Use(disposable));

            await container.DisposeAsync();

            disposable.WasDisposed.ShouldBeTrue();
        }

        [Fact]
        public void
            something_in_the_middle_of_container_that_tries_to_dispose_container_will_not_blow_everything_up_with_a_stack_overflow_exception
            ()
        {
            var container =
                new Container(x =>
                {
                    x.ForSingletonOf<ITryToDisposeContainer>().Use<ITryToDisposeContainer>();
                });

            // just want it spun up
            container.GetInstance<ITryToDisposeContainer>();

            container.Dispose();
        }
        
        [Fact]
        public async Task
            Async_something_in_the_middle_of_container_that_tries_to_dispose_container_will_not_blow_everything_up_with_a_stack_overflow_exception
            ()
        {
            var container =
                new Container(x =>
                {
                    x.ForSingletonOf<ITryToDisposeContainer>().Use<ITryToDisposeContainer>();
                });

            // just want it spun up
            container.GetInstance<ITryToDisposeContainer>();

            await container.DisposeAsync();
        }

        public class ITryToDisposeContainer : IDisposable
        {
            private readonly IContainer _container;

            public ITryToDisposeContainer(IContainer container)
            {
                _container = container;
            }

            public void Dispose()
            {
                _container.Dispose();
            }
        }

        [Fact]
        public void main_container_should_dispose_singletons()
        {
            var container = new Container(x => { x.ForSingletonOf<C1Yes>().Use<C1Yes>(); });

            var single = container.GetInstance<C1Yes>();

            container.Dispose();

            single.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task main_container_should_async_dispose_singletons()
        {
            var container = new Container(x => { x.ForSingletonOf<C1Yes>().Use<C1Yes>(); });

            var single = container.GetInstance<C1Yes>();

            await container.DisposeAsync();

            single.WasDisposed.ShouldBeTrue();
        }

        [Fact]
        public void disposing_a_nested_container_does_not_try_to_dispose_objects_created_by_the_parent()
        {
            var container = new Container(x => { x.ForSingletonOf<I1>().Use<C1No>(); });

            var child = container.GetNestedContainer();

            // Blows up if the Dispose() is called
            var notDisposable = child.GetInstance<I1>();

            child.Dispose();
        }
        
        [Fact]
        public async Task async_disposing_a_nested_container_does_not_try_to_dispose_objects_created_by_the_parent()
        {
            var container = new Container(x => { x.ForSingletonOf<I1>().Use<C1No>(); });

            var child = container.GetNestedContainer();

            // Blows up if the Dispose() is called
            var notDisposable = child.GetInstance<I1>();

            await child.DisposeAsync();
        }

        [Fact]
        public void
            disposing_a_nested_container_should_dispose_all_of_the_transient_objects_created_by_the_nested_container()
        {
            var container = new Container(x =>
            {
                x.For<I1>().Use<C1Yes>();
                x.For<I2>().Use<C2Yes>();


                x.For<I3>().Add<C3Yes>().Named("1");
                x.For<I3>().Add<C3Yes>().Named("2");

            });

            var child = container.GetNestedContainer();

            var disposables = new[]
            {
                child.GetInstance<I1>().As<Disposable>(),
                child.GetInstance<I2>().As<Disposable>(),
                child.GetInstance<I3>("1").As<Disposable>(),
                child.GetInstance<I3>("2").As<Disposable>()
            };

            child.Dispose();

            foreach (var disposable in disposables)
            {
                disposable.WasDisposed.ShouldBeTrue();
            }
        }
        
        [Fact]
        public async Task
            disposing_a_nested_container_should_async_dispose_all_of_the_transient_objects_created_by_the_nested_container()
        {
            var container = new Container(x =>
            {
                x.For<I1>().Use<C1Yes>();
                x.For<I2>().Use<C2Yes>();


                x.For<I3>().Add<C3Yes>().Named("1");
                x.For<I3>().Add<C3Yes>().Named("2");

            });

            var child = container.GetNestedContainer();

            var disposables = new[]
            {
                child.GetInstance<I1>().As<Disposable>(),
                child.GetInstance<I2>().As<Disposable>(),
                child.GetInstance<I3>("1").As<Disposable>(),
                child.GetInstance<I3>("2").As<Disposable>()
            };

            await child.DisposeAsync();

            foreach (var disposable in disposables)
            {
                disposable.WasDisposed.ShouldBeTrue();
            }
        }


        [Fact]
        public void should_dispose_objects_injected_into_the_container_2()
        {
            var container = new Container(x => x.For<I1>().Use<C1Yes>()).GetNestedContainer();

            var disposable = container.GetInstance<I1>().ShouldBeOfType<C1Yes>();

            container.Dispose();

            disposable.WasDisposed.ShouldBeTrue();
        }
        
        

        [Fact]
        public async Task should_async_dispose_objects_injected_into_the_container_2()
        {
            var container = new Container(x => x.For<I1>().Use<C1Yes>()).GetNestedContainer();

            var disposable = container.GetInstance<I1>().ShouldBeOfType<C1Yes>();

            await container.DisposeAsync();

            disposable.WasDisposed.ShouldBeTrue();
        }

        [Fact]
        public void should_dispose_lambda_instance_with_transient_scope()
        {
            var container = new Container(x => { x.For<I1>().Use(_ => new C1Yes()).Transient(); });

            var disposableDependent = container.GetInstance<DisposableDependent>();
            disposableDependent.WasDisposed.ShouldBeFalse();

            container.Dispose();

            disposableDependent.WasDisposed.ShouldBeTrue();
            disposableDependent.ChildDisposable.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task should_async_dispose_lambda_instance_with_transient_scope()
        {
            var container = new Container(x => { x.For<I1>().Use(_ => new C1Yes()).Transient(); });

            var disposableDependent = container.GetInstance<DisposableDependent>();
            disposableDependent.WasDisposed.ShouldBeFalse();

            await container.DisposeAsync();

            disposableDependent.WasDisposed.ShouldBeTrue();
            disposableDependent.ChildDisposable.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void should_work_when_get_disposables_in_parallel()
        {
            var container = new Container(x => { x.For<I1>().Use(_ => new C1Yes()).Transient(); });

            Parallel.For(0, 1000,
                (i) =>
                {
                    container.GetInstance<DisposableDependent>();
                });
            
            container.Dispose();
        }
        
        [Fact]
        public async Task should_work_when_get_disposables_async_in_parallel()
        {
            var container = new Container(x => { x.For<I1>().Use(_ => new C1Yes()).Transient(); });

            Parallel.For(0, 1000,
                (i) =>
                {
                    container.GetInstance<DisposableDependent>();
                });
            
            await container.DisposeAsync();
        }
        
        [Fact]
        public void should_dispose_lambda_instance_with_transient_scope_parent_is_singleton()
        {
            var container = new Container(x =>
            {
                x.For<DisposableDependent>().Use<DisposableDependent>().Singleton();
                x.For<I1>().Use(_ => new C1Yes()).Transient();
            });

            var disposableDependent = container.GetInstance<DisposableDependent>();
            disposableDependent.WasDisposed.ShouldBeFalse();

            container.Dispose();

            disposableDependent.WasDisposed.ShouldBeTrue();
            disposableDependent.ChildDisposable.WasDisposed.ShouldBeTrue();
        }
        

        [Fact]
        public async Task should_async_dispose_lambda_instance_with_transient_scope_parent_is_singleton()
        {
            var container = new Container(x =>
            {
                x.For<DisposableDependent>().Use<DisposableDependent>().Singleton();
                x.For<I1>().Use(_ => new C1Yes()).Transient();
            });

            var disposableDependent = container.GetInstance<DisposableDependent>();
            disposableDependent.WasDisposed.ShouldBeFalse();

            await container.DisposeAsync();

            disposableDependent.WasDisposed.ShouldBeTrue();
            disposableDependent.ChildDisposable.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void should_dispose_lambda_instance_with_transient_scope_parent_is_scoped()
        {
            var container = new Container(x =>
            {
                x.For<DisposableDependent>().Use<DisposableDependent>().Scoped();
                x.For<I1>().Use(_ => new C1Yes()).Transient();
            });

            var disposableDependent = container.GetInstance<DisposableDependent>();
            disposableDependent.WasDisposed.ShouldBeFalse();

            container.Dispose();

            disposableDependent.WasDisposed.ShouldBeTrue();
            disposableDependent.ChildDisposable.WasDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task should_async_dispose_lambda_instance_with_transient_scope_parent_is_scoped()
        {
            var container = new Container(x =>
            {
                x.For<DisposableDependent>().Use<DisposableDependent>().Scoped();
                x.For<I1>().Use(_ => new C1Yes()).Transient();
            });

            var disposableDependent = container.GetInstance<DisposableDependent>();
            disposableDependent.WasDisposed.ShouldBeFalse();

            await container.DisposeAsync();

            disposableDependent.WasDisposed.ShouldBeTrue();
            disposableDependent.ChildDisposable.WasDisposed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(DisposalLock.Ignore)]
        [InlineData(DisposalLock.Unlocked)]
        public void should_clear_disposables_collection_when_disposed(DisposalLock disposalLock)
        {
            var container = new Container(x =>
            {
                x.For<Disposable>().Use<Disposable>();
            });

            var service = container.GetInstance<Disposable>();
            container.DisposalLock = disposalLock;
            container.Dispose();

            container.AllDisposables.ShouldBeEmpty();
        }
        
        [Theory]
        [InlineData(DisposalLock.Ignore)]
        [InlineData(DisposalLock.Unlocked)]
        public async Task should_clear_disposables_collection_when_async_disposed(DisposalLock disposalLock)
        {
            var container = new Container(x =>
            {
                x.For<Disposable>().Use<Disposable>();
            });

            var service = container.GetInstance<Disposable>();
            container.DisposalLock = disposalLock;

            #region sample_calling_async_disposable
            // Asynchronously disposing the container
            await container.DisposeAsync();

            #endregion

            container.AllDisposables.ShouldBeEmpty();
        }
        
                [Theory]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Object, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Transient)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Transient)]
        public void when_disposing_with_only_async_disposables(RegistrationType registration, ServiceLifetime lifetime)
        {
            var container = Container.For(services =>
            {
                switch (registration)
                {
                    case RegistrationType.Lambda:
                        services.For<OnlyAsyncDisposable>().Use(s => new OnlyAsyncDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Object:
                        services.For<OnlyAsyncDisposable>().Use(new OnlyAsyncDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Type:
                        services.For<OnlyAsyncDisposable>().Use<OnlyAsyncDisposable>()
                            .Lifetime = lifetime;
                        break;
                }
                
                
            });

            var item = container.GetInstance<OnlyAsyncDisposable>();
            
            container.Dispose();
            
            item.WasAsyncDisposed.ShouldBeTrue();
        }
        
        [Theory]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Object, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Transient)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Transient)]
        public async Task when_async_disposing_with_only_async_disposables(RegistrationType registration, ServiceLifetime lifetime)
        {
            var container = Container.For(services =>
            {
                switch (registration)
                {
                    case RegistrationType.Lambda:
                        services.For<OnlyAsyncDisposable>().Use(s => new OnlyAsyncDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Object:
                        services.For<OnlyAsyncDisposable>().Use(new OnlyAsyncDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Type:
                        services.For<OnlyAsyncDisposable>().Use<OnlyAsyncDisposable>()
                            .Lifetime = lifetime;
                        break;
                }
                
                
            });

            var item = container.GetInstance<OnlyAsyncDisposable>();
            
            await container.DisposeAsync();
            
            item.WasAsyncDisposed.ShouldBeTrue();
        }
        
        
        [Theory]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Object, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Transient)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Transient)]
        public void when_disposing_with_dual_disposables(RegistrationType registration, ServiceLifetime lifetime)
        {
            var container = Container.For(services =>
            {
                switch (registration)
                {
                    case RegistrationType.Lambda:
                        services.For<BothDisposable>().Use(s => new BothDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Object:
                        services.For<BothDisposable>().Use(new BothDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Type:
                        services.For<BothDisposable>().Use<BothDisposable>()
                            .Lifetime = lifetime;
                        break;
                }
                
                
            });

            var item = container.GetInstance<BothDisposable>();
            
            container.Dispose();
            
            item.WasDisposed.ShouldBeTrue();
            item.WasAsyncDisposed.ShouldBeFalse();
        }
        
        [Theory]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Object, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Singleton)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Scoped)]
        [InlineData(RegistrationType.Lambda, ServiceLifetime.Transient)]
        [InlineData(RegistrationType.Type, ServiceLifetime.Transient)]
        public async Task when_async_disposing_with_both_disposables(RegistrationType registration, ServiceLifetime lifetime)
        {
            var container = Container.For(services =>
            {
                switch (registration)
                {
                    case RegistrationType.Lambda:
                        services.For<BothDisposable>().Use(s => new BothDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Object:
                        services.For<BothDisposable>().Use(new BothDisposable())
                            .Lifetime = lifetime;
                        break;
                    
                    case RegistrationType.Type:
                        services.For<BothDisposable>().Use<BothDisposable>()
                            .Lifetime = lifetime;
                        break;
                }
                
                
            });

            var item = container.GetInstance<BothDisposable>();
            
            await container.DisposeAsync();
            
            item.WasAsyncDisposed.ShouldBeTrue();
            item.WasDisposed.ShouldBeFalse();
        }
    }

    public class DisposableDependent : IDisposable
    {
        private I1 _i1;
        private bool _wasDisposed;

        public DisposableDependent(I1 i1)
        {
            _i1 = i1;
        }

        public C1Yes ChildDisposable
        {
            get { return (C1Yes)_i1; }
        }

        public bool WasDisposed
        {
            get { return _wasDisposed; }
        }

        public void Dispose()
        {
            Assert.False(_wasDisposed, "This object should not be disposed twice");
            _wasDisposed = true;
        }
    }

    public class Disposable : IDisposable
    {
        private bool _wasDisposed;

        public bool WasDisposed
        {
            get { return _wasDisposed; }
        }

        public void Dispose()
        {
            Assert.False(_wasDisposed, "This object should not be disposed twice");

            _wasDisposed = true;
        }
    }

    public class NotDisposable : IDisposable
    {
        public void Dispose()
        {
            Assert.True(false, "This object should not be disposed");
        }
    }

    public interface I1
    {
    }

    public interface I2
    {
    }

    public interface I3
    {
    }

    public class C1Yes : Disposable, I1
    {
        
    }

    public class C1No : NotDisposable, I1
    {
    }

    public class C2Yes : Disposable, I2
    {
    }

    public class C2No : Disposable, I2
    {
    }

    public class C3Yes : Disposable, I3
    {
    }

    public class C3No : Disposable, I3
    {
    }
    
    public class OnlyAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            WasAsyncDisposed = true;
            return ValueTask.CompletedTask;
        }

        public bool WasAsyncDisposed { get; set; }
    }

    public class BothDisposable : IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public ValueTask DisposeAsync()
        {
            WasAsyncDisposed = true;
            return ValueTask.CompletedTask;
        }

        public bool WasAsyncDisposed { get; set; }
    }
}