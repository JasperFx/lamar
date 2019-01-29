using System;
using System.Net.Http;
using System.Threading.Tasks;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using UserApp.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace LamarRest.Testing
{
    public interface IUserService
    {
        [Get("/user/{name}")]
        Task<User> GetUser(string name);

        [Post("/user/create")]
        Task Create(User user);
    }
    
    public class end_to_end 
    {
        private readonly ITestOutputHelper _output;

        public end_to_end(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task lets_do_it()
        {
            var container = new Container(x =>
            {
                x.AddHttpClient(typeof(IUserService).Name, c => c.BaseAddress = new Uri("http://localhost:5000"));
                x.Policies.Add<LamarRestPolicy>();
            });

            var userService = container.GetInstance<IUserService>();
            userService
                .ShouldNotBeNull();
            
            var user1 = new User
            {
                Name = "DarthVader"
            };

            await userService.Create(user1);

            var user2 = await userService.GetUser("DarthVader");
            
            user2.Name.ShouldBe("DarthVader");
        }

    }
}