using System.Net.Http;
using Lamar;
using LamarRest.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using UserApp.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace LamarRest.Testing
{
    public class code_compilation
    {
        private readonly ITestOutputHelper _output;

        public code_compilation(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void try_it_out()
        {
            var container = new Container(x =>
            {
                x.AddSingleton(Substitute.For<IConfiguration>());
                x.AddSingleton(Substitute.For<IHttpClientFactory>());

            });

            var serviceType = GeneratedServiceType.For(typeof(IUserService), container);
            
            _output.WriteLine(serviceType.SourceCode);
        }
    }
}