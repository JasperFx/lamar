using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lamar.Codegen;
using Lamar.Microsoft.DependencyInjection;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using Baseline;
using Microsoft.EntityFrameworkCore;

namespace Lamar.Testing.AspNetCoreIntegration
{
    public class integration_with_aspnetcore
    {
        [Fact]
        public void default_registrations_for_service_provider_factory()
        {
            var container = Container.For(x => x.AddLamar());

            container.Model.DefaultTypeFor<IServiceProviderFactory<ServiceRegistry>>()
                .ShouldBe(typeof(LamarServiceProviderFactory));

            container.Model.DefaultTypeFor<IServiceProviderFactory<IServiceCollection>>()
                .ShouldBe(typeof(LamarServiceProviderFactory));
        }

        [Fact]
        public void trouble_shoot_kestrel_setup_problems()
        {
            var container = new Container(services =>
            {
                //services.TryAddSingleton<ITransportFactory, LibuvTransportFactory>();

                services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();
                //services.AddSingleton<IServer, KestrelServer>();
                services.AddOptions();
            });

            container.GetInstance<IOptions<KestrelServerOptions>>();
        }

        [Fact]
        public void integration_with_ef()
        {
            var container = new Container(_ =>
            {
                _.AddDbContext<AppDbContext>(opts => { opts.UseSqlServer("connection string"); });
            });

            container.GetInstance<AppDbContext>().ShouldNotBeNull();
            
            container.GetInstance<Foo>().Context.ShouldNotBeNull();
        }

        public class Foo
        {
            public AppDbContext Context { get; }

            public Foo(AppDbContext context)
            {
                Context = context;
            }
        }
        


        [Fact]
        public void use_in_app()
        {
            var builder = new WebHostBuilder();
            builder
                .UseLamar()
            
                .UseUrls("http://localhost:5002")
                .UseServer(new NulloServer())
                .UseApplicationInsights()
                .UseStartup<Startup>();

            var failures = new List<Type>();

            using (var host = builder.Start())
            {
                var container = host.Services.ShouldBeOfType<Container>();


                var errors = container.Model.AllInstances.Where(x => x.ErrorMessages.Any())
                    .SelectMany(x => x.ErrorMessages).ToArray();

                if (errors.Any()) throw new Exception(errors.Join(", "));




                foreach (var instance in container.Model.AllInstances.Where(x => !x.ServiceType.IsOpenGeneric()))
                {
                    instance.Resolve(container).ShouldNotBeNull();
                    
//                    try
//                    {
//                        
//                    }
//                    catch (Exception e)
//                    {
//                        failures.Add(instance.ServiceType);
//                    }
                }
            }

            if (failures.Any())
            {
                throw new Exception(failures.Select(x => x.FullNameInCode()).Join(Environment.NewLine));
            }
        }

    }

    public class AppDbContext : DbContext
    {
        protected AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
    }
    
    public class NulloServer : IServer
    {
        public void Dispose()
        {
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IFeatureCollection Features { get; } = new FeatureCollection();
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


            services.AddAuthentication().AddFacebook(facebookOptions =>
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
    })
    
    public class Config {
        public static IEnumerable<ApiResource> GetApiResources() => new List<ApiResource> {
            new ApiResource("api1", "My API")
        };

        public static IEnumerable<Client> GetClients() => new List<Client> {
            new Client {
                ClientId = "client",

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // secret for authentication
                ClientSecrets = {
                    new Secret("secret".Sha256())
                },

                // scopes that client has access to
                AllowedScopes = {
                    "api1"
                }
            }
        };
    }

    public interface IMessageMaker
    {
    }

    public class MessageMaker : IMessageMaker
    {
        private readonly string _message;

        public MessageMaker(string message)
        {
            _message = message;
        }

        public override string ToString()
        {
            return _message;
        }
    }
}