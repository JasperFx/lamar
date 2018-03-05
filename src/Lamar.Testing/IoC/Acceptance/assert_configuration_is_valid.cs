using System;
using Lamar.IoC;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class assert_configuration_is_valid
    {
        [Fact]
        public void happy_path()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget, AWidget>();
                _.AddTransient<WidgetUser>();
                _.AddTransient<IWidget, BlueWidget>();
                _.AddTransient<WidgetIListUser>();
            });
            
            container.AssertConfigurationIsValid();
        }
        
        [Fact]
        public void unhappy_path_with_missing_dependency()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<WidgetUser>();
            });
            
            Exception<ContainerValidationException>.ShouldBeThrownBy(() =>
            {
                container.AssertConfigurationIsValid();
            }).Message.ShouldContain("IWidget is not registered");
            
        }
        
        [Fact]
        public void happy_path_when_doing_light_validation()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget, AWidget>();
                _.AddTransient<ThingThatBlowsUp>();
            });
            
            container.AssertConfigurationIsValid(AssertMode.ConfigOnly);
        }
        
        [Fact]
        public void sad_path_with_runtime_exception()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget, AWidget>();
                _.AddTransient<ThingThatBlowsUp>();
            });

            var ex = Exception<ContainerValidationException>.ShouldBeThrownBy(() =>
            {
                container.AssertConfigurationIsValid(AssertMode.Full);
            });
            
            ex.Message.ShouldContain("Error in new ThingThatBlowsUp(IWidget)");
            ex.Message.ShouldContain("DivideByZeroException");
        }
        
        [Fact]
        public void using_validation_method_happy_path()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget, BlueWidget>();
                _.AddTransient<WidgetUserThatOnlyLikesBlue>();
            });
            
            container.AssertConfigurationIsValid();
        }
        
        [Fact]
        public void using_validation_method_sad_path()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget, RedWidget>();
                _.AddTransient<WidgetUserThatOnlyLikesBlue>();
            });

            Exception<ContainerValidationException>.ShouldBeThrownBy(() =>
            {
                container.AssertConfigurationIsValid();
            }).Message.ShouldContain("Only Blue");
            
        }
    }

    public class ThingThatBlowsUp
    {
        public ThingThatBlowsUp(IWidget widget)
        {
            throw new DivideByZeroException();
        }
    }

    public class WidgetUserThatOnlyLikesBlue
    {
        private readonly IWidget _widget;

        public WidgetUserThatOnlyLikesBlue(IWidget widget)
        {
            _widget = widget;
        }

        [ValidationMethod]
        public void OnlyBlue()
        {
            if (_widget is BlueWidget) return;
            
            throw new Exception("Only Blue");
        }
    }
}