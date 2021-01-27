using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Lamar.Microsoft.DependencyInjection;
using Xunit;
using Shouldly;
using Newtonsoft.Json;
using Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit.Abstractions;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem
{
    public class IntegrationTestsMicrosoftDI : IClassFixture<CustomWebApplicationFactory<MicrosoftDIStartup>>
    {
        private readonly WebApplicationFactory<MicrosoftDIStartup> _factory;

        public IntegrationTestsMicrosoftDI(CustomWebApplicationFactory<MicrosoftDIStartup> factory)
        {
            factory.UseLamar = false;
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(@"src\Lamar.AspNetCoreTests.Integration");
                builder.ConfigureTestServices(services =>
                {
                    services.AddControllers();
                });
            });
        }

    }

    public class IntegrationTestsLamar : IClassFixture<CustomWebApplicationFactory<LamarStartup>>
    {
        private readonly WebApplicationFactory<LamarStartup> _factory;
        private readonly ITestOutputHelper _output;

        public IntegrationTestsLamar(CustomWebApplicationFactory<LamarStartup> factory, ITestOutputHelper output)
        {
            _output = output;

            factory.UseLamar = true;
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(@"src/Lamar.AspNetCoreTests.Integration");
                builder.ConfigureTestServices(services =>
                {
                    services.AddControllers();
                });
            });
        }

        [Fact]
        public async void ControllerRequest_ExecutesInParallel_WithoutExceptions()
        {
            var client = _factory.CreateClient();
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(client.GetAsync("Book/InsertAndReturn"));
            }

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task HealthCheckRequest_completes_successfully()
        {
            var client = _factory.CreateClient();

            for (int i = 0; i < 20; ++i)
            {
                var result = await client.GetAsync("health").ConfigureAwait(false);

                var responseString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseObject = JsonConvert.DeserializeObject<SerializableHealthCheckResult>(responseString);

                if (responseObject.Status != HealthStatus.Healthy)
                {
                    var unhealthyEntries = responseObject.Entries.Where(entry => entry.Status != HealthStatus.Healthy).ToList();
                    foreach (var unhealthyEntry in unhealthyEntries)
                    {
                        _output.WriteLine($"Unhealth Entry ({unhealthyEntry.Name}): {unhealthyEntry.Description}\n{unhealthyEntry.Exception}\n");
                    }

#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                }

                responseObject.Status.ShouldBe(HealthStatus.Healthy);
            }
        }
    }
}
