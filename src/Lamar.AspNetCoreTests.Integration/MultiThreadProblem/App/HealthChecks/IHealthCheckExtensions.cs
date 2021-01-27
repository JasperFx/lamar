using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App.HealthChecks
{
	public static class IHealthCheckExtensions
	{
		public static IServiceCollection AddTestHealthChecks(this IServiceCollection services)
		{
			services.AddHttpContextAccessor();
			var healthCheckBuilder = services.AddHealthChecks();

			healthCheckBuilder.AddDbContextCheck<Context>();

			foreach (var index in Enumerable.Range(0, 20))
			{
				// Add multiple test health checks to ensure concurrent resolver access doesn't blow up
				healthCheckBuilder.AddTestHealthCheck($"TestHealthCheck{index}");
			}

			return services;
		}

		public static IHealthChecksBuilder AddTestHealthCheck(this IHealthChecksBuilder builder, string registrationName)
		{
			builder.Add(new HealthCheckRegistration(
				registrationName,
				(serviceProvider) =>
				{
					try
					{
						// Get some different objects from the service provider to try and trigger any thread
						// safety issues in the container resolution logic
						var healthCheckLock = serviceProvider.GetRequiredService<HealthCheckLock>();
						healthCheckLock.DoWorkInsideLock();

						var testObject1 = serviceProvider.GetRequiredService<HealthCheckTestObject1>();
						var testObject2 = serviceProvider.GetRequiredService<HealthCheckTestObject2>();
						var testObject3 = serviceProvider.GetRequiredService<HealthCheckTestObject3>();
						return new SuccessHealthCheck(registrationName);
					}
					catch (Exception exc)
					{
						return new SetupFailedHealthCheck(exc, registrationName);
					}
				},
				HealthStatus.Unhealthy,
				default));

			return builder;
		}

		public static string CreateHealthReportPlainText(string key, HealthReportEntry entry)
		{
			var entryOutput = new StringBuilder($"{key}: {entry.Status} | {entry.Duration}\n");
			if (entry.Tags?.Any() == true)
			{
				entryOutput.Append("-  Tags:");
				entryOutput.Append(string.Join(", ", entry.Tags));
				entryOutput.Append('\n');
			}

			if (!string.IsNullOrWhiteSpace(entry.Description))
			{
				entryOutput.Append($"-  Description: {entry.Description}\n\n");
			}

			if (entry.Exception != null)
			{
				entryOutput.Append($"-  Exception: {entry.Exception}\n\n");
			}

			if (entry.Data?.Count > 0)
			{
				entryOutput.Append("-  Data:\n");
				foreach (var keyValuePair in entry.Data)
				{
					entryOutput.Append($"\t{keyValuePair.Key}: {keyValuePair.Value}");
				}
				entryOutput.Append('\n');
			}

			return entryOutput.ToString();
		}

		public static IEndpointRouteBuilder MapTestHealthChecks(this IEndpointRouteBuilder endpoints)
		{
			endpoints.MapHealthChecks("/health", new HealthCheckOptions
			{
				ResponseWriter = async (context, report) =>
				{
					var serializableReport = new SerializableHealthCheckResult(report);
					var resultString = JsonConvert.SerializeObject(serializableReport);

					context.Response.ContentType = "application/json";
					await context.Response.WriteAsync(resultString).ConfigureAwait(false);
				}
			});

			return endpoints;
		}
	}
}
