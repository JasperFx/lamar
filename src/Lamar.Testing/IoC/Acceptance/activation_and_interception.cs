using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class activation_and_interception
    {
        public class ActivatedWidget : IWidget, IActivated
        {
            public void DoSomething()
            {
                
            }
            
            public bool WasActivated { get; set; }
        }

        public class WidgetHolder
        {
            public readonly IList<IWidget> Widgets = new List<IWidget>();
        }

        public class DecoratedWidget : IWidget
        {
            public IWidget Inner { get; }

            public DecoratedWidget(IWidget inner)
            {
                Inner = inner;
            }

            public void DoSomething()
            {
            }
        }
        
        [Fact]
        public void simple_activation_of_one_instance()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<ActivatedWidget>().Named("yes")
                    .OnCreation(w => w.WasActivated = true);

                x.For<IWidget>().Add<ActivatedWidget>().Named("no");
            });
            
            container.GetInstance<IWidget>("yes")
                .ShouldBeOfType<ActivatedWidget>()
                .WasActivated.ShouldBeTrue();
            
            container.GetInstance<IWidget>("no")
                .ShouldBeOfType<ActivatedWidget>()
                .WasActivated.ShouldBeFalse();
        }

        [Fact]
        public void simple_activation_of_one_instance_using_service_context()
        {
            var container = new Container(x =>
            {
                x.ForConcreteType<WidgetHolder>().Configure.Singleton();
                x.For<IWidget>().Add<ActivatedWidget>().Named("yes")
                    .OnCreation((c, w) =>
                    {
                        c.GetInstance<WidgetHolder>().Widgets.Add(w);
                    });

                x.For<IWidget>().Add<ActivatedWidget>().Named("no");
            });
            
            var yes = container.GetInstance<IWidget>("yes");
            var no = container.GetInstance<IWidget>("no");
            
            container.GetInstance<WidgetHolder>()
                .Widgets.Single().ShouldBeSameAs(yes);

        }

        #region sample_intercept_a_single_instance

        [Fact]
        public void intercept_a_single_instance()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<ActivatedWidget>()
                    .Named("yes")
                    .OnCreation(w => new DecoratedWidget(w));

                x.For<IWidget>().Add<ActivatedWidget>().Named("no");
            });

            container.GetInstance<IWidget>("yes")
                .ShouldBeOfType<DecoratedWidget>()
                .Inner.ShouldBeOfType<ActivatedWidget>();

            container.GetInstance<IWidget>("no")
                .ShouldBeOfType<ActivatedWidget>();

        }

        #endregion
        
        [Fact]
        public void intercept_a_single_transient_instance_as_dependency()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<ActivatedWidget>()
                    .Named("yes")
                    .OnCreation(w => new DecoratedWidget(w));
            });

            container.GetInstance<WidgetCatcher>()
                .Widget
                .ShouldBeOfType<DecoratedWidget>()
                .Inner.ShouldBeOfType<ActivatedWidget>();

        }

        public class WidgetCatcher
        {
            public IWidget Widget { get; }

            public WidgetCatcher(IWidget widget)
            {
                Widget = widget;
            }
        }
        
        [Fact]
        public void intercept_a_single_instance_as_singleton()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<ActivatedWidget>()
                    .Singleton()
                    .Named("yes")
                    .OnCreation(w => new DecoratedWidget(w));

                x.For<IWidget>().Add<ActivatedWidget>().Named("no");
            });

            container.GetInstance<IWidget>("yes")
                .ShouldBeOfType<DecoratedWidget>()
                .Inner.ShouldBeOfType<ActivatedWidget>();

            container.GetInstance<IWidget>("no")
                .ShouldBeOfType<ActivatedWidget>();

        }
        
        [Fact]
        public void intercept_a_single_instance_as_scoped()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Add<ActivatedWidget>()
                    .Scoped()
                    .Named("yes")
                    .OnCreation(w => new DecoratedWidget(w));

                x.For<IWidget>().Add<ActivatedWidget>().Named("no");
            });

            container.GetInstance<IWidget>("yes")
                .ShouldBeOfType<DecoratedWidget>()
                .Inner.ShouldBeOfType<ActivatedWidget>();

            container.GetInstance<IWidget>("no")
                .ShouldBeOfType<ActivatedWidget>();

        }

        [Fact]
        public void intercept_by_service_type()
        {
           
            var container = new Container(services =>
            {
                services.AddSingleton<IWidget>(new AWidget());
                services.AddTransient<IWidget, AWidget>();
                services.AddScoped<IWidget, BWidget>();
                
                services.For<IWidget>()
                    .InterceptAll(x => new DecoratedWidget(x));
            });

            var widgets = container.GetAllInstances<IWidget>();
            
            widgets.All(x => x is DecoratedWidget).ShouldBeTrue();
        }
        
        [Fact]
        public void intercept_by_service_type_and_check_scope()
        {
           
            var container = new Container(services =>
            {
                services.For<IWidget>().Use<AWidget>().Named("singleton").Singleton();
                services.For<IWidget>().Use<AWidget>().Named("scoped").Scoped();
                services.For<IWidget>().Use<AWidget>().Named("transient").Transient();

                services.For<IWidget>().InterceptAll(x => new DecoratedWidget(x));
            });

            var singleton = container.GetInstance<IWidget>("singleton");
            singleton.ShouldBeSameAs(container.GetInstance<IWidget>("singleton"));

            var scoped = container.GetInstance<IWidget>("scoped");
            scoped.ShouldBeSameAs(container.GetInstance<IWidget>("scoped"));

            using var nested = container.GetNestedContainer();
            var scopeFromNested = nested.GetInstance<IWidget>("scoped");
            scopeFromNested.ShouldBeSameAs(nested.GetInstance<IWidget>("scoped"));
            scopeFromNested.ShouldNotBeSameAs(scoped);
            
            container.GetInstance<IWidget>("transient")
                .ShouldNotBeSameAs(container.GetInstance<IWidget>("transient"));
        }

        public interface IActivated
        {
            bool WasActivated { get; set; }
        }

        public class ActivatedService : IService, IActivated
        {
            public void DoSomething()
            {
                
            }

            public bool WasActivated { get; set; }
        }
        

        [Fact]
        public void intercept_by_either_service_type_or_implementation_type()
        {
            var container = new Container(services =>
            {
                services.AddTransient<IWidget, ActivatedWidget>();
                services.AddTransient<IService, ActivatedService>();

                services.For<IActivated>().OnCreationForAll(a => a.WasActivated = true);
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<ActivatedWidget>()
                .WasActivated.ShouldBeTrue();
            
            container.GetInstance<IService>()
                .ShouldBeOfType<ActivatedService>()
                .WasActivated.ShouldBeTrue();
        }


        public interface IPoller : IDisposable
        {
       
        }

        #region sample_poller

        public class Poller : IPoller
        {
            public void Start()
            {
                // start the actual polling
            }
            
            public void Dispose()
            {
                // stop polling
            }
        }

        #endregion

        public static void examples()
        {
            #region sample_using_activator_on_one_registration

            var container = new Container(x =>
            {
                x.For<IPoller>().Use<Poller>()
                    
                    // This registers an activator on just this
                    // one registration
                    .OnCreation(poller => poller.Start());
            });

            #endregion
        }

        #region sample_IStartable

        public interface IStartable : IDisposable
        {
            void Start();
        }

        #endregion

        #region sample_StartablePoller

        public class StartablePoller : IPoller, IStartable
        {
            public void Dispose()
            {
                // shut things down
            }

            public void Start()
            {
                // start up the polling
            }
        }
        #endregion

        public static void startable_polling_registration()
        {
            #region sample_activator_by_marker_type

            var container = new Container(services =>
            {
                // Remember that Lamar natively understands .Net
                // DI registrations w/o any adapter
                services.AddSingleton<IPoller, StartablePoller>();
                
                // Other registrations that might include other 
                // IStartable types
                
                services.For<IStartable>()
                    .OnCreationForAll(x => x.Start());
            });

            #endregion  
        }

        #region sample_StartableTracker

        public class StartableTracker
        {
            public List<IStartable> Startables { get; }
                = new List<IStartable>();
        }

        #endregion

        public static void startable_and_tracker_registration()
        {
            #region sample_startable_and_tracker_registration

            var container = new Container(services =>
            {
                services.AddSingleton<IPoller, StartablePoller>();
                
                // Other registrations that might include other 
                // IStartable types
                
                services.AddSingleton<StartableTracker>();

                services.For<IStartable>()
                    .OnCreationForAll((context, startable) =>
                    {
                        var tracker = context.GetInstance<StartableTracker>();
                        tracker.Startables.Add(startable);

                        startable.Start();
                    });
            });

            #endregion
        }
    }
}