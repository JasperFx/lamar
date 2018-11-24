using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Baseline;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Lamar.Testing.AspNetCoreIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace MemoryUsageChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new WebHostBuilder();
            builder
                .UseLamar()

                .UseUrls("http://localhost:5002")
                .UseServer(new NulloServer())
                .UseApplicationInsights()
                .UseStartup<Startup>();


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            long bootstrappingTime;

            long starting;
            long ending;

            using (var host = builder.Start())
            {
                var container = host.Services.As<Container>();
                bootstrappingTime = stopwatch.ElapsedMilliseconds;


                var errors = container.Model.AllInstances.Where(x => x.Instance.ErrorMessages.Any())
                    .SelectMany(x => x.Instance.ErrorMessages).ToArray();

                if (errors.Any()) throw new Exception(errors.Join(", "));



                starting = stopwatch.ElapsedMilliseconds;
                foreach (var instance in container.Model.AllInstances.Where(x => !x.ServiceType.IsOpenGeneric()))
                {
                    instance.Resolve().ShouldNotBeNull();
                }

                ending = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();

                var writer = new StringWriter();

                container.Bootstrapping.DisplayTimings().Write(writer);

                Console.WriteLine(writer.ToString());
                
                Console.WriteLine($"Total");
            }
        }
    }
    
    public class Startup
    {
        public void ConfigureContainer(ServiceRegistry services)
        {
            services.AddMvc();
            services.AddLogging();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());
            
            services.For<IMessageMaker>().Use(new MessageMaker("Hey there."));

            services.AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "auth";
                    options.RequireHttpsMetadata = true;
                })


                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = "something";
                    facebookOptions.AppSecret = "else";
                });


        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServer();

            app.Run(c =>
            {
                var maker = c.RequestServices.GetService<IMessageMaker>();
                return c.Response.WriteAsync(maker.ToString());
            });
        }
    }

}