using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget3;

namespace Lamar.Testing.Samples
{
    public class GettingStarted
    {
        public void start_a_container()
        {
            #region sample_start-a-container
            var container = new Container(x =>
            {
                // Using StructureMap style registrations
                x.For<IClock>().Use<Clock>();
                
                // Using ASP.Net Core DI style registrations
                x.AddTransient<IClock, Clock>();
                
                // and lots more services in all likelihood
            });
            #endregion

            #region sample_resolving-services-quickstart
            // StructureMap style
            
            // Get a required service
            var clock = container.GetInstance<IClock>();
            
            // Try to resolve a service if it's registered
            var service = container.TryGetInstance<IService>();

            // ASP.Net Core style
            var provider = (IServiceProvider)container;
            
            // Get a required service
            var clock2 = provider.GetRequiredService<IClock>();
            
            // Try to resolve a service if it's registered
            var service2 = provider.GetService<IService>();
            #endregion
        }
    }
}