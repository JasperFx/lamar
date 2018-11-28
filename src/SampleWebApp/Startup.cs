using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication4
{
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
            //services.Scan(s =>
            //{
            //    s.TheCallingAssembly();
            //    s.WithDefaultConventions();
            //});
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
