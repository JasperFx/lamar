using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;

namespace Lamar.Testing.Samples
{
    public class NestedContainer
    {
        #region sample_using-nested-container
        [Fact]
        public void using_nested_containers()
        {
            var container = new Container(x =>
            {
                x.AddSingleton<IWidget, AWidget>();
                x.AddScoped<IService, WhateverService>();
                x.AddTransient<IClock, Clock>();
            });

            var rootWidget = container.GetInstance<IWidget>();
            var rootService = container.GetInstance<IService>();

            var nested = container.GetNestedContainer();
            
            // Singleton scoped objects are the same
            nested.GetInstance<IWidget>()
                .ShouldBeSameAs(rootWidget);
            
            // Scoped objects are specific to the container
            var nestedService = nested.GetInstance<IService>();
            nestedService
                .ShouldNotBeSameAs(rootService);
            
            nested.GetInstance<IService>()
                .ShouldBeSameAs(nestedService);
        }
        #endregion
        
    }
}