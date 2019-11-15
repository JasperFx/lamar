using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem
{
    public class CustomWebApplicationFactory<T> : WebApplicationFactory<T> where T : class
    {
        public bool UseLamar { get; set; }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<T>());
            return UseLamar ? builder.UseLamar() : builder;
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(null)
                .UseStartup<T>();
        }
    }
}
