using Baseline.Reflection;
using LamarCompiler;
using LamarRest.Internal;
using Microsoft.AspNetCore.SignalR.Internal;
using Shouldly;
using UserApp.Controllers;
using Xunit;

namespace LamarRest.Testing.Internal
{
    public class GeneratedTypeTests
    {
        [Fact]
        public void get_input_type_positive()
        {
            var method = ReflectionHelper.GetMethod<IUserService>(x => x.Create(null));

            GeneratedServiceType.DetermineRequestType(method).ShouldBe(typeof(User));
        }

        [Fact]
        public void get_input_type_negative()
        {
            var method = ReflectionHelper.GetMethod<IUserService>(x => x.GetUser(null));

            GeneratedServiceType.DetermineRequestType(method).ShouldBeNull();
        }

        [Fact]
        public void get_response_type_positive()
        {
            var method = ReflectionHelper.GetMethod<IUserService>(x => x.GetUser(null));
            GeneratedServiceType.DetermineResponseType(method)
                .ShouldBe(typeof(User));
            
            
        }
        
        [Fact]
        public void get_response_type_negative()
        {
            var method = ReflectionHelper.GetMethod<IUserService>(x => x.Create(null));
            GeneratedServiceType.DetermineResponseType(method)
                .ShouldBeNull();
            
            
        }
    }
}