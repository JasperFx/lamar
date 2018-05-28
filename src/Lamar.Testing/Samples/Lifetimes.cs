using Lamar.Testing.IoC.Acceptance;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;

namespace Lamar.Testing.Samples
{
    // SAMPLE: LifetimeRegistry
    public class LifetimeRegistry : ServiceRegistry
    {
        public LifetimeRegistry()
        {
            // Lifetimes the ASP.Net Core way
            // The registration methods are all extension
            // methods, so hence, "this."
            this.AddTransient<IWidget, AWidget>();

            this.AddSingleton<IClock, Clock>();

            this.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Lifetimes the old StructureMap way
            // Transient is the default
            For<IWidget>().Use<AWidget>();

            For<IClock>().Use<Clock>().Singleton();
            
            // or

            ForSingletonOf<IClock>().Use<Clock>();

            For<IUnitOfWork>().Use<UnitOfWork>().Scoped();
        }
    }
    // ENDSAMPLE
}