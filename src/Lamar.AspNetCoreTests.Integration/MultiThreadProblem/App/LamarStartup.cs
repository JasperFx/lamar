using Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
	public class LamarStartup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<Context>();
			services.AddDbContext<SecondContext>();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

			services.AddTestHealthChecks();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseRouting();
			app.UseEndpoints(configure =>
			{
				configure.MapControllers();
				configure.MapTestHealthChecks();
			});
		}

		public void ConfigureContainer(ServiceRegistry services)
		{
			services.For<IBookService>().Use<BookService>().Transient();
			services.For<IOtherService>().Use<OtherService>().Transient();
			services.For<IContextFactory>().Use<ContextFactory>().Transient();

			services.For<HealthCheckLock>().Use<HealthCheckLock>().Scoped();
			services.For<HealthCheckTestObject1>().Use<HealthCheckTestObject1>().Scoped();
			services.For<HealthCheckTestChild1>().Use<HealthCheckTestChild1>().Scoped();
			services.For<HealthCheckTestObject2>().Use<HealthCheckTestObject2>().Scoped();
			services.For<HealthCheckTestChild2>().Use<HealthCheckTestChild2>().Scoped();
			services.For<HealthCheckTestObject3>().Use<HealthCheckTestObject3>().Scoped();
		}
	}
}
