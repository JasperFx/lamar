using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using LamarCodeGeneration.Util;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oakton.AspNetCore;

namespace WebApplication4
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            #region sample_using-oakton-aspnetcore
            var registry = new ServiceRegistry();
            registry.Scan(x =>
            {
                x.Assembly(typeof(Program).Assembly);
                x.WithDefaultConventions();
            });
            
            var builder = new HostBuilder();
            
            return builder
                // Replaces the built in DI container
                // with Lamar
                .UseLamar(registry)
                .ConfigureWebHostDefaults(x =>
                {
                    // Normal ASP.Net Core bootstrapping
                    x.UseUrls("http://localhost:5002")
                        .UseKestrel()
                        .UseStartup<Startup>();
                })


                .RunOaktonCommands(args);
            #endregion
            

        }
    }
}
