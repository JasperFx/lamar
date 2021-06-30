using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Lamar.AspNetCoreTests
{
    public class Bug_286
    {
        [Fact]
        public void do_not_blow_up()
        {
            using var host = Host.CreateDefaultBuilder(new string[0])
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .Build();


            var container = (IContainer)host.Services;

            container.GetInstance<UserManager<Startup.MyIdentityUser>>().ShouldNotBeNull();
            container.GetInstance<SignInManager<Startup.MyIdentityUser>>().ShouldNotBeNull();
        }
        
        
public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public class MyIdentityUser :  IdentityUser<string>
        {
            
        }

        public class MyIdentityRole:  IdentityRole<string>
        {
            
        }

        public class AuthIdentityDbContext : DbContext
        {
            
        }
        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureContainer(ServiceRegistry services)
        {
            services.AddDbContext<AuthIdentityDbContext>();
            
            services
                .AddIdentity<MyIdentityUser, MyIdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<AuthIdentityDbContext>();;

            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
        
    }
}