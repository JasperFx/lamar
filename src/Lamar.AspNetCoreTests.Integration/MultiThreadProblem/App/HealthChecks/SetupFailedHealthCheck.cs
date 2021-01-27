using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App.HealthChecks
{
	public class SetupFailedHealthCheck : IHealthCheck
	{
		private readonly Exception _exception;
		private readonly string _registrationName;

		public SetupFailedHealthCheck(Exception exception, string registrationName)
		{
			_exception = exception;
			_registrationName = registrationName;
		}

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new HealthCheckResult(
				HealthStatus.Unhealthy,
				description: $"An exception occurred while attempting to construct the health check for registration: {_registrationName}",
				exception: _exception
			));
		}
	}
}
