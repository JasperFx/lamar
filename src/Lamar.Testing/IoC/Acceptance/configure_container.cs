using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class configure_container
    {
        #region sample_add_all_new_services
        [Fact]
        public void add_all_new_services()
        {
            var container = new Container(_ => { _.AddTransient<IWidget, RedWidget>(); });
            
            container.Configure(_ => _.AddTransient<IService, WhateverService>());

            container.GetInstance<IService>()
                .ShouldBeOfType<WhateverService>();
        }
        #endregion

        [Fact]
        public void add_consumed_service_later()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<WidgetUser>();
            });
            container.Configure(services =>
            {
                services.AddTransient<IWidget, RedWidget>();
            });
            var instance = container.GetInstance<WidgetUser>();
            instance.ShouldBeOfType<WidgetUser>();
            instance.Widget.ShouldBeOfType<RedWidget>();
        }

        [Fact]
        public void add_to_existing_family()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget, RedWidget>();
            });
            
            container.Configure(_ =>
            {
                _.AddTransient<IWidget, BlueWidget>();
                _.AddTransient<IWidget, GreenWidget>();
            });
            
            container.GetInstance<IWidget>()
                .ShouldBeOfType<GreenWidget>();
            
            container.GetAllInstances<IWidget>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(RedWidget), typeof(BlueWidget), typeof(GreenWidget));

        }
        
        [Fact]
        public void bug_74_trygetinstance_then_configure()
        {
            var container = Container.Empty();
            
            container.TryGetInstance<IWidget>().ShouldBeNull();
            
            container.Configure(x => x.AddTransient<IWidget, AWidget>());

            container.GetInstance<IWidget>().ShouldBeOfType<AWidget>();
        }
        
        [Fact]
        public void configure_overwrites_default()
        {
            var container = Container.For(x => x.AddTransient<IWidget, AWidget>());

            container.GetInstance<IWidget>().ShouldBeOfType<AWidget>();
            
            container.Configure(x => x.AddTransient<IWidget, BWidget>());
            
            container.GetInstance<IWidget>().ShouldBeOfType<BWidget>();
        }
    }
}