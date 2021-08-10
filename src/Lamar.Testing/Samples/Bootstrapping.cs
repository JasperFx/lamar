using System.Collections.Generic;
using Lamar.Testing.IoC.Acceptance;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;

namespace Lamar.Testing.Samples
{
    public class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        
    }
    
    public class Bootstrapping
    {
        public static void Go()
        {
            
            #region sample_bootstrap-inline
            var container = new Container(x =>
            {
                x.AddTransient<IClock, Clock>();
            });
            #endregion
            
            
            
            
        }

        public static void Go2()
        {
            #region sample_bootstrap-with-registry
            // Create a Lamar.ServiceRegistry object
            // and define your service registrations
            var registry = new ServiceRegistry();
            
            // Use ASP.Net Core style registrations
            // for basic functionality
            registry.AddSingleton<IClock, Clock>();
            registry.AddTransient<IWidget, RedWidget>();
            
            // Or use StructureMap style registration syntax
            // as an alternative or to use more advanced usage
            registry.For<IClockFactory>()
                .Use<ClockFactory>()
                .Singleton();
            
            
            var container = new Container(registry);
            #endregion
        }
    }
}