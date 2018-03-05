using Lamar.IoC;
using Lamar.IoC.Resolvers;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class resolver_base_class_tests
    {
        public readonly Scope theScope = Scope.Empty();
        
        [Fact]
        public void non_disposable_singleton()
        {
            var singleton1 = new Singleton1(theScope);

            var clock = singleton1.Resolve(Scope.Empty());
            clock
                .ShouldBeSameAs(singleton1.Resolve(Scope.Empty()));
            
            theScope.Disposables.ShouldNotContain(clock);

        }
        
        [Fact]
        public void disposable_singleton()
        {
            var singleton1 = new DisposableSingleton(theScope);

            var clock = singleton1.Resolve(Scope.Empty());
            clock
                .ShouldBeSameAs(singleton1.Resolve(Scope.Empty()));
            
            theScope.Disposables.ShouldContain(clock);

                
        }
        
        [Fact]
        public void scoped_resolver()
        {
            var resolver = new ScopedClock();

            var clock = resolver.Resolve(theScope);
            clock.ShouldBeSameAs(resolver.Resolve(theScope));

            clock.ShouldNotBeTheSameAs(resolver.Resolve(Scope.Empty()));
            
        }
        
        [Fact]
        public void scoped_resolver_will_track_disposables()
        {
            var resolver = new DisposableScopedClock();
            var clock = resolver.Resolve(theScope);
            
            theScope.Disposables.ShouldContain(clock);
        }
        
        [Fact]
        public void transient_resolver_behavior()
        {
            var resolver = new TransientClock();

            resolver.Resolve(theScope)
                .ShouldNotBeTheSameAs(resolver.Resolve(theScope));
        }
        
        [Fact]
        public void transient_disposables()
        {
            var resolver = new DisposableTransientClock();

            var clock = resolver.Resolve(theScope);
            
            theScope.Disposables.ShouldContain(clock);

        }
    }

    public class TransientClock : TransientResolver<IClock>
    {
        public override IClock Build(Scope scope)
        {
            return new Clock();
        }
    }

    public class DisposableTransientClock : TransientResolver<IClock>
    {
        public override IClock Build(Scope scope)
        {
            return new DisposableClock();
        }
    }

    public class ScopedClock : ScopedResolver<IClock>
    {
        public override IClock Build(Scope scope)
        {
            return new Clock();
        }
       
    }

    public class DisposableScopedClock : ScopedResolver<IClock>
    {
        public override IClock Build(Scope scope)
        {
            return new DisposableClock();
        }
    }

    [LamarIgnore]
    public class Singleton1 : SingletonResolver<IClock>
    {
        public Singleton1(Scope topLevelScope) : base(topLevelScope)
        {
        }

        public override IClock Build(Scope scope)
        {
            return new Clock();
        }
    }
    
    [LamarIgnore]
    public class DisposableSingleton : SingletonResolver<IClock>
    {
        public DisposableSingleton(Scope topLevelScope) : base(topLevelScope)
        {
        }

        public override IClock Build(Scope scope)
        {
            return new DisposableClock();
        }
    }
}