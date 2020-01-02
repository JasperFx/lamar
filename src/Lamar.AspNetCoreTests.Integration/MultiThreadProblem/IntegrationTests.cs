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

        public IntegrationTestsLamar(CustomWebApplicationFactory<LamarStartup> factory)
        {
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
        public async void ExecutesInParallel_WithoutExceptions()
        {
            var client = _factory.CreateClient();
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(client.GetAsync("Book/InsertAndReturn"));
            }

            await Task.WhenAll(tasks);
        }
    }
}
