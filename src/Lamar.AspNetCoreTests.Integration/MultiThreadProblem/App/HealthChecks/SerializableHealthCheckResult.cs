using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App.HealthChecks
{
	public class SerializableHealthCheckResult
	{
		// Default constructor for json serialization / deserialization support
		public SerializableHealthCheckResult() { }

		public SerializableHealthCheckResult(HealthReport healthReport)
		{
			_ = healthReport ?? throw new ArgumentNullException(nameof(healthReport));

			Status = healthReport.Status;
			TotalDuration = healthReport.TotalDuration;

			if (healthReport.Entries != null)
			{
				Entries = healthReport.Entries.Select(entry => new SerializableHealthCheckResultEntry(entry.Value, entry.Key)).ToList();
			}
		}

		public List<SerializableHealthCheckResultEntry> Entries { get; set; }
		public HealthStatus Status { get; set; }
		public TimeSpan TotalDuration { get; set; }
	}

	public class SerializableHealthCheckResultEntry
	{
		// Default constructor for json serialization / deserialization support
		public SerializableHealthCheckResultEntry() { }

		public SerializableHealthCheckResultEntry(HealthReportEntry entry, string name)
		{
			Description = entry.Description;
			Duration = entry.Duration;
			Exception = entry.Exception?.ToString();
			Name = name;
			Status = entry.Status;
			Tags = entry.Tags?.ToList();
		}

		public string Description { get; set; }
		public TimeSpan Duration { get; set; }
		public string Exception { get; set; }
		public string Name { get; set; }
		public HealthStatus Status { get; set; }
		public List<string> Tags { get; set; }
	}
}
