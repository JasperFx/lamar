using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class LamarStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Context>();
            services.AddDbContext<SecondContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(configure => configure.MapControllers());
        }

        public void ConfigureContainer(ServiceRegistry services)
        {
            services.For<IBookService>().Use<BookService>().Transient();
            services.For<IOtherService>().Use<OtherService>().Transient();
            services.For<IContextFactory>().Use<ContextFactory>().Transient();
        }
    }
}
