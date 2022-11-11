using System;
using System.Linq;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class decorators
    {
        [Fact]
        public void decorator_example()
        {
            #region sample_decorator-sample
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                
                // The AWidget type will be decorated w/ 
                // WidgetHolder when you resolve it from the container
                _.For<IWidget>().Use<AWidget>();
                
                _.For<IThing>().Use<Thing>();
            });

            // Just proving that it actually works;)
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();
            
            #endregion
        }

        public class WidgetHolderDecoratorPolicy : DecoratorPolicy
        {
            public WidgetHolderDecoratorPolicy() : base(typeof(IWidget), typeof(WidgetDecorator))
            {
            }
        }
        
        [Fact]
        public void decorator_example_alternative_registration()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.Policies.DecorateWith<WidgetHolderDecoratorPolicy>();
                _.For<IWidget>().Use<AWidget>();
                _.For<IThing>().Use<Thing>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void multiple_decorators()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IWidget>().DecorateAllWith<OtherWidgetHolder>();
                _.For<IWidget>().Use<AWidget>();
                _.For<IThing>().Use<Thing>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<OtherWidgetHolder>()
                .Inner.ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void multiple_decorators_with_type_registrations()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For(typeof(IWidget)).DecorateAllWith(typeof(WidgetDecorator));
                _.For(typeof(IWidget)).DecorateAllWith(typeof(OtherWidgetHolder));
                
                _.For<IWidget>().Use<AWidget>();
                _.For<IThing>().Use<Thing>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<OtherWidgetHolder>()
                .Inner.ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void decorator_is_applied_on_configure_when_it_is_a_new_family()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IThing>().Use<Thing>();
            });
            
            container.Configure(x => x.AddTransient<IWidget, BWidget>());
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<BWidget>();
        }
        
        [Fact]
        public void decorator_is_applied_on_configure_when_it_is_an_existing_family()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IThing>().Use<Thing>();
                _.For<IWidget>().Use<AWidget>();
            });
            
            container.Configure(x => x.AddTransient<IWidget, BWidget>());
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<BWidget>();
        }
        
        [Fact]
        public void copy_the_name_and_lifetime_from_the_inner_as_transient()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IWidget>().Use<AWidget>().Named("A").Transient();
                _.For<IThing>().Use<Thing>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();

            var @default = container.Model.For<IWidget>().Default.Instance.ShouldBeOfType<ConstructorInstance>();
            @default.Name.ShouldBe("A");
            @default.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }
        
        [Fact]
        public void copy_the_name_and_lifetime_from_the_inner()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IWidget>().Use<AWidget>().Named("A").Singleton();
                _.For<IThing>().Use<Thing>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();

            var @default = container.Model.For<IWidget>().Default.Instance.ShouldBeOfType<ConstructorInstance>();
            @default.Name.ShouldBe("A");
            @default.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }
        
        [Fact]
        public void copy_the_name_and_lifetime_from_the_inner_2()
        {
            var container = new Container(_ =>
            {
                // This usage adds the WidgetHolder as a decorator
                // on all IWidget registrations and makes AWidget
                // the default
                _.For<IWidget>().DecorateAllWith<WidgetDecorator>();
                _.For<IWidget>().Use<AWidget>().Named("A").Scoped();
                _.For<IThing>().Use<Thing>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<WidgetDecorator>()
                .Inner.ShouldBeOfType<AWidget>();

            var @default = container.Model.For<IWidget>().Default.Instance.ShouldBeOfType<ConstructorInstance>();
            @default.Name.ShouldBe("A");
            @default.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
        
        [Fact]
        public void decorate_with_open_generics()
        {
            typeof(DecoratedFoo<,>).MakeGenericType(typeof(IWidget), typeof(IService))
                .ShouldNotBeNull();
            
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<IService>().Use<AService>();

                x.For(typeof(IFoo<,>)).DecorateAllWith(typeof(DecoratedFoo<,>));

                x.For(typeof(IFoo<,>)).Use(typeof(Foo<,>));
            });

            var decorator = container.GetInstance<IFoo<IWidget, IService>>()
                .ShouldBeOfType<DecoratedFoo<IWidget, IService>>();

            decorator.One.ShouldBeOfType<AWidget>();
            decorator.Two.ShouldBeOfType<AService>();
        }

        
        #region sample_WidgetHolder-Decorator
        public class WidgetDecorator : IWidget
        {
            public WidgetDecorator(IThing thing, IWidget inner)
            {
                Inner = inner;
            }

            public IWidget Inner { get; }
            
            public void DoSomething()
            {
                // do something before 
                Inner.DoSomething();
                // do something after
            }
        }
        #endregion

        public class OtherWidgetHolder : WidgetDecorator
        {
            public OtherWidgetHolder(IThing thing, IWidget inner2) : base(thing, inner2)
            {
            }
        }
        
        public interface IFoo<T1, T2>
        {
            T1 One { get; }
            T2 Two { get; }
        }

        public class Foo<T1, T2> : IFoo<T1, T2>
        {
            private readonly T1 _one;
            private readonly T2 _two;

            public Foo(T1 one, T2 two)
            {
                _one = one;
                _two = two;
            }

            public T1 One
            {
                get { return _one; }
            }

            public T2 Two
            {
                get { return _two; }
            }
        }

        public class DecoratedFoo<T1, T2> : IFoo<T1, T2>
        {
            private readonly IFoo<T1, T2> _inner;

            public DecoratedFoo(IFoo<T1, T2> inner)
            {
                _inner = inner;
            }

            public T1 One
            {
                get { return _inner.One; }
            }

            public T2 Two
            {
                get { return _inner.Two; }
            }
        }

        public interface IService
        {
        }

        public class AService : IService
        {
        }

        public class BService : IService
        {
        }


    }
}