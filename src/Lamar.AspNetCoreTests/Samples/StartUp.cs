using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Testing.AspNetCoreIntegration.Samples
{

    public static class Program
    {
        /*
        // SAMPLE: getting-started-main
        public static void Main(string[] args)
        {
            var builder = new WebHostBuilder();
            builder
                // Replaces the built in DI container
                // with Lamar
                .UseLamar()
                
                // Normal ASP.Net Core bootstrapping
                .UseUrls("http://localhost:5002")
                .UseKestrel()
                .UseStartup<Startup>();

            builder.Start();

        }
        // ENDSAMPLE
        */
    }
    
    // SAMPLE: getting-started-startup
    public class Startup
    {
        // Take in Lamar's ServiceRegistry instead of IServiceCollection
        // as your argument, but fear not, it implements IServiceCollection
        // as well
        public void ConfigureContainer(ServiceRegistry services)
        {
            // Supports ASP.Net Core DI abstractions
            services.AddMvc();
            services.AddLogging();
            
            // Also exposes Lamar specific registrations
            // and functionality
            services.Scan(s =>
            {
                s.TheCallingAssembly();
                s.WithDefaultConventions();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
    // ENDSAMPLE
}