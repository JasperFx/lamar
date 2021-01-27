using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App.HealthChecks
{
	public class SuccessHealthCheck : IHealthCheck
	{
		private readonly string _registrationName;
		private readonly HealthCheckTestObjects1 _testObjects1;
		private readonly HealthCheckTestObjects2 _testObjects2;

		public SuccessHealthCheck(string registrationName, HealthCheckTestObjects1 testObjects1, HealthCheckTestObjects2 testObjects2)
		{
			_registrationName = registrationName;
			_testObjects1 = testObjects1 ?? throw new ArgumentNullException(nameof(testObjects1));
			_testObjects2 = testObjects2 ?? throw new ArgumentNullException(nameof(testObjects2));
		}

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new HealthCheckResult(
				HealthStatus.Healthy,
				description: $"Health check successful for: {_registrationName}"
			));
		}
	}

	public class HealthCheckTestObjects1
	{
		private readonly HealthCheckTestChild1 _child1;

		public HealthCheckTestObjects1(HealthCheckTestChild1 child1)
		{
			_child1 = child1 ?? throw new ArgumentNullException(nameof(child1));
		}
	}

	public class HealthCheckTestChild1
	{
		private readonly HealthCheckTestChild2 _child2;

		public HealthCheckTestChild1(HealthCheckTestChild2 child2)
		{
			_child2 = child2 ?? throw new ArgumentNullException(nameof(child2));
		}
	}

	public class HealthCheckTestChild2
	{
		private readonly HealthCheckTestChild3 _child3;

		public HealthCheckTestChild2(HealthCheckTestChild3 child3)
		{
			_child3 = child3 ?? throw new ArgumentNullException(nameof(child3));
		}
	}

	public class HealthCheckTestChild3
	{
		private readonly Context _context;

		public HealthCheckTestChild3(Context context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
	}

	public class HealthCheckTestObjects2
	{
		public HealthCheckTestObjects2()
		{
			// Simulate some cpu bound setup work...
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			while (stopWatch.ElapsedMilliseconds < 5000)
			{
				continue;
			}
			stopWatch.Stop();
		}
	}
}
