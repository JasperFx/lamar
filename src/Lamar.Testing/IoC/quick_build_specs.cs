using System;
using System.Collections.Generic;
using Lamar.IoC.Instances;
using Lamar.Testing.IoC.Acceptance;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class quick_build_specs
    {
        [Fact]
        public void happy_path()
        {
            var clock = new Clock();
            var widget = new AWidget();
            
            var container = Container.For(_ =>
            {
                _.AddSingleton<IClock>(clock);
                _.AddSingleton<IWidget>(widget);
            });

            var user = container.QuickBuild<WidgetUser>();
            user.Clock.ShouldBeSameAs(clock);
            user.Widget.ShouldBeSameAs(widget);

        }
        
        [Fact]
        public void uses_greediest_constructor_it_can()
        {
            var clock = new Clock();
            var widget = new AWidget();
            
            var container = Container.For(_ =>
            {
                _.AddSingleton<IClock>(clock);
                //_.AddSingleton<IWidget>(widget);
            });
            
            var user = container.QuickBuild<WidgetUser>();
            user.Clock.ShouldBeSameAs(clock);
            user.Widget.ShouldBeNull();
        }
        
        [Fact]
        public void throw_if_no_suitable_constructor()
        {
            var clock = new Clock();
            var widget = new AWidget();
            
            var container = Container.For(_ =>
            {
                _.AddSingleton<IClock>(clock);
                _.AddSingleton<IWidget>(widget);
            });

            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                container.QuickBuild<GuyWithStringArg>();
            }).Message.ShouldContain(ConstructorInstance.NoPublicConstructorCanBeFilled);
        }
        
        [Fact]
        public void throw_if_no_public_constructors()
        {
            var clock = new Clock();
            var widget = new AWidget();
            
            var container = Container.For(_ =>
            {
                _.AddSingleton<IClock>(clock);
                _.AddSingleton<IWidget>(widget);
            });

            Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
            {
                container.QuickBuild<GuyWithNoPublicConstructors>();
            }).Message.ShouldContain(ConstructorInstance.NoPublicConstructors);
        }
        
        [Fact]
        public void throw_if_not_concrete()
        {
            var clock = new Clock();
            var widget = new AWidget();
            
            var container = Container.For(_ =>
            {
                _.AddSingleton<IClock>(clock);
                _.AddSingleton<IWidget>(widget);
            });

            Exception<InvalidOperationException>.ShouldBeThrownBy(() => { container.QuickBuild<IWidget>(); });
        }
        
        [Fact]
        public void with_dependency_on_list()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<GreenWidget>();
                _.For<IWidget>().Use<BlueWidget>().Singleton();
                _.For<IWidget>().Use<RedWidget>().Scoped();
                
            });

            var user1 = container.QuickBuild<WidgetListUser>();
            var user2 = container.QuickBuild<WidgetListUser>();

            user1.Widgets[0].ShouldBeOfType<GreenWidget>();
            user1.Widgets[1].ShouldBeOfType<BlueWidget>();
            user1.Widgets[2].ShouldBeOfType<RedWidget>();
            
            user1.Widgets[1].ShouldBeSameAs(user2.Widgets[1]);
            user1.Widgets[2].ShouldBeSameAs(user2.Widgets[2]);
        }
        
        [Fact]
        public void with_dependency_on_array()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<GreenWidget>();
                _.For<IWidget>().Use<BlueWidget>().Singleton();
                _.For<IWidget>().Use<RedWidget>().Scoped();
                
            });

            var user1 = container.QuickBuild<WidgetArrayUser>();
            var user2 = container.QuickBuild<WidgetArrayUser>();

            user1.Widgets[0].ShouldBeOfType<GreenWidget>();
            user1.Widgets[1].ShouldBeOfType<BlueWidget>();
            user1.Widgets[2].ShouldBeOfType<RedWidget>();
            
            user1.Widgets[1].ShouldBeSameAs(user2.Widgets[1]);
            user1.Widgets[2].ShouldBeSameAs(user2.Widgets[2]);
        }

        public class GreenWidget : IWidget
        {
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }

        public class BlueWidget : IWidget
        {
            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }
        
        [Fact]
        public void honor_named_attribute_on_parameter()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<GreenWidget>().Named("green");
                _.For<IWidget>().Use<BlueWidget>().Named("blue");
            });

            container.GetInstance<IWidget>().ShouldBeOfType<BlueWidget>();

            container.QuickBuild<SelectiveWidgetUser>()
                .Widget.ShouldBeOfType<GreenWidget>();
        }
    }

    public class GuyWithNoPublicConstructors
    {
        private GuyWithNoPublicConstructors()
        {
        }
    }

    public class GuyWithStringArg
    {
        public GuyWithStringArg(IWidget widget, string arg)
        {
        }
    }

    public class SelectiveWidgetUser
    {
        public IWidget Widget { get; }

        public SelectiveWidgetUser([Named("green")]IWidget widget)
        {
            Widget = widget;
        }
    }
    
    public class WidgetArrayUser
    {
        public IWidget[] Widgets { get; }

        public WidgetArrayUser(IWidget[] widgets)
        {
            Widgets = widgets;
        }
    }

    public class WidgetListUser
    {
        public IList<IWidget> Widgets { get; }

        public WidgetListUser(IList<IWidget> widgets)
        {
            Widgets = widgets;
        }
    }
    
    public class WidgetUser
    {
        public IWidget Widget { get; }
        public IClock Clock { get; }

        public WidgetUser(IWidget widget, IClock clock)
        {
            Widget = widget;
            Clock = clock;
        }

        public WidgetUser(IClock clock)
        {
            Clock = clock;
        }
    }
    
}