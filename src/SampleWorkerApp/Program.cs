using System;
using Lamar.Microsoft.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleWorkerApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		#region sample_startup-worker-service
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseLamar()
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<Worker>();
				})
				.ConfigureContainer<Lamar.ServiceRegistry>((context, services) =>
				{
					// Also exposes Lamar specific registrations
					// and functionality
					services.Scan(s =>
					{
						s.TheCallingAssembly();
						s.WithDefaultConventions();
					});
				});
		#endregion
	}
}
