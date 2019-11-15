using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class MicrosoftDIStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Context>();
            services.AddDbContext<SecondContext>();

            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IOtherService, OtherService>();
            services.AddTransient<IContextFactory, ContextFactory>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(configure => configure.MapControllers());
        }
    }
}
