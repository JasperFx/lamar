using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Testing.Samples
{
    public class GettingStarted
    {
        public void start_a_container()
        {
            // SAMPLE: start-a-container
            var container = new Container(x =>
            {
                // Using StructureMap style registrations
                x.For<IClock>().Use<Clock>();
                
                // Using ASP.Net Core DI style registrations
                x.AddTransient<IClock, Clock>();
                
                // and lots more services in all likelihood
            });
            // ENDSAMPLE

            // SAMPLE: resolving-services-quickstart
            // StructureMap style
            var clock = container.GetInstance<IClock>();

            // ASP.Net Core style
            var clock2 = container.GetService<IClock>();
            // ENDSAMPLE
        }
    }
}