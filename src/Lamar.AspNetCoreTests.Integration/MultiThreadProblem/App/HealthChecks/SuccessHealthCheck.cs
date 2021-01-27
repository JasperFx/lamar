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

		public SuccessHealthCheck(string registrationName)
		{
			_registrationName = registrationName;
		}

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new HealthCheckResult(
				HealthStatus.Healthy,
				description: $"Health check successful for: {_registrationName}"
			));
		}
	}

	public class HealthCheckTestObject1
	{
		private readonly HealthCheckTestChild1 _child1;

		public HealthCheckTestObject1(HealthCheckTestChild1 child1)
		{
			_child1 = child1 ?? throw new ArgumentNullException(nameof(child1));
		}
	}

	public class HealthCheckTestChild1
	{
		public HealthCheckTestChild1()
		{
		}
	}

	public class HealthCheckTestObject2
	{
		private readonly HealthCheckTestChild2 _child2;

		public HealthCheckTestObject2(HealthCheckTestChild2 child2)
		{
			_child2 = child2 ?? throw new ArgumentNullException(nameof(child2));
		}
	}

	public class HealthCheckTestChild2
	{
		private readonly Context _context;

		public HealthCheckTestChild2(Context context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
	}

	public class HealthCheckTestObject3
	{
		public HealthCheckTestObject3()
		{
		}
	}

	public class HealthCheckLock
	{
		private object _lock = new object();
		private long? _result = null;

		public long DoWorkInsideLock()
		{
			if (_result != null)
			{
				return _result.Value;
			}

			lock (_lock)
			{
				if (_result != null)
				{
					return _result.Value;
				}

				var stopWatch = new Stopwatch();
				stopWatch.Start();
				while (stopWatch.ElapsedMilliseconds < 2000)
				{
					continue;
				}
				stopWatch.Stop();
				_result = stopWatch.ElapsedMilliseconds;

				return _result.Value;
			}
		}
	}
}
