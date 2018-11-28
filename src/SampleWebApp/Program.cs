using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebApplication4
{
    public class Program
    {
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

            var host = builder.Build();
            
            host.Run();

        }
    }
}
