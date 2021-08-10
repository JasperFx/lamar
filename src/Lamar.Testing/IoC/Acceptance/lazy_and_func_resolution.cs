using System;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
public class lazy_and_func_resolution
    {
        [Fact]
        public void resolve_func_by_type()
        {
            var container = Container.For(_ => _.AddTransient<IWidget, AWidget>());

            var func = container.GetInstance<Func<IWidget>>();
            
            func.ShouldNotBeNull();

            func().ShouldBeOfType<AWidget>();
        }

        public class FuncUser
        {
            private readonly Func<IWidget> _func;

            public FuncUser(Func<IWidget> func)
            {
                _func = func;
            }

            public IWidget Build()
            {
                return _func();
            }
        }
        
        [Fact]
        public void use_func_by_type_as_dependency()
        {
            var container = Container.For(_ => _.AddTransient<IWidget, AWidget>());

            var user = container.GetInstance<FuncUser>();

            user.Build().ShouldBeOfType<AWidget>();
        }

        #region sample_Lazy-in-usage
        public class WidgetLazyUser
        {
            private readonly Lazy<IWidget> _widget;

            public WidgetLazyUser(Lazy<IWidget> widget)
            {
                _widget = widget;
            }

            public IWidget Widget
            {
                get { return _widget.Value; }
            }
        }

        [Fact]
        public void lazy_resolution_in_action()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();
            });

            container.GetInstance<WidgetLazyUser>()
                .Widget.ShouldBeOfType<AWidget>();
        }

        #endregion

        [Fact]
        public void can_build_a_Lazy_of_T_automatically()
        {
            Container.Empty().GetInstance<Lazy<ConcreteClass>>()
                .Value.ShouldBeOfType<ConcreteClass>().ShouldNotBeNull();
        }

        [Fact]
        public void build_a_func_for_a_concrete_class()
        {
            var container = Container.Empty();
            var func = container.GetInstance<Func<ConcreteClass>>();

            func().ShouldNotBeNull();
        }

        [Fact]
        public void build_a_func_that_returns_a_transient()
        {
            var container =
                new Container(x => x.For<IWidget>().Use<AWidget>());

            var func = container.GetInstance<Func<IWidget>>();
            var w1 = func();
            var w2 = func();
            var w3 = func();

            w1.ShouldBeOfType<AWidget>();

            w1.ShouldNotBeTheSameAs(w2);
            w1.ShouldNotBeTheSameAs(w3);
            w2.ShouldNotBeTheSameAs(w3);
        }

        #region sample_using-func-t
        [Fact]
        public void build_a_func_that_returns_a_singleton()
        {
            var container = new Container(x =>
            {
                x.ForSingletonOf<IWidget>().Use<AWidget>();
            });

            var func = container.GetInstance<Func<IWidget>>();
            var w1 = func();
            var w2 = func();
            var w3 = func();

            w1.ShouldBeOfType<AWidget>();

            w1.ShouldBeSameAs(w2);
            w1.ShouldBeSameAs(w3);
            w2.ShouldBeSameAs(w3);
        }

        #endregion

        public class GreenWidget : IWidget
        {
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }
        public class BlueWidget : IWidget{
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }
        public class RedWidget : IWidget{
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }
        
        #region sample_using-func-string-t
        [Fact]
        public void build_a_func_by_string()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<GreenWidget>().Named("green");
                x.For<IWidget>().Add<BlueWidget>().Named("blue");
                x.For<IWidget>().Add<RedWidget>().Named("red");
            });

            var func = container.GetInstance<Func<string, IWidget>>();
            func("green").ShouldBeOfType<GreenWidget>();
            func("blue").ShouldBeOfType<BlueWidget>();
            func("red").ShouldBeOfType<RedWidget>();
        }

        #endregion


        [Fact]
        public void build_a_func_by_string_from_parent()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<GreenWidget>().Named("green");
                x.For<IWidget>().Add<RedWidget>().Named("red");
            });

            var parent = container.GetInstance<ConcreteWidgetUser>();
            parent.Use("red").ShouldBeOfType<RedWidget>();
        }

        public class ConcreteWidgetUser
        {
            private readonly Func<string, IWidget> _widgetFactory;

            public ConcreteWidgetUser(Func<string, IWidget> widgetFactory)
            {
                _widgetFactory = widgetFactory;
            }

            public IWidget Use(string color)
            {
                return _widgetFactory(color);
            }
        }

        public class ConcreteClass
        {
        }

        #region sample_using-lazy-as-workaround-for-bidirectional-dependency

        public class Thing1
        {
            private readonly Lazy<Thing2> _thing2;

            public Thing1(Lazy<Thing2> thing2)
            {
                _thing2 = thing2;
            }

            public Thing2 Thing2
            {
                get { return _thing2.Value; }
            }
        }

        public class Thing2
        {
            public Thing1 Thing1 { get; set; }

            public Thing2(Thing1 thing1)
            {
                Thing1 = thing1;
            }
        }

        [Fact]
        public void use_lazy_as_workaround_for_bi_directional_dependency()
        {
            var container = Container.For(_ =>
            {
                _.AddSingleton<Thing1>();
                _.AddSingleton<Thing2>();
            });
            
            var thing1 = container.GetInstance<Thing1>();
            var thing2 = container.GetInstance<Thing2>();

            thing1.Thing2.ShouldBeSameAs(thing2);
            thing2.Thing1.ShouldBeSameAs(thing1);
        }

        #endregion
    }
}