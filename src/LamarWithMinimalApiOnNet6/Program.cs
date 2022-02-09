using System;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

#region sample_using_lamar_with_minimal_api

var builder = WebApplication.CreateBuilder(args);

// use Lamar as DI.
builder.Host.UseLamar((context, registry) =>
{
    // register services using Lamar
    registry.For<ITest>().Use<MyTest>();
    registry.IncludeRegistry<MyRegistry>();

    // add the controllers
    registry.AddControllers();
});


var app = builder.Build();
app.MapControllers();

app.MapGet("/", (ITest service) => service.SayHello());

app.Run();

        #endregion

[Route("api/[controller]")]
[ApiController]
public class HelloController : ControllerBase
{
    private readonly ITest _test;
    private readonly ITestTime _testTime;

    public HelloController(ITest test, ITestTime testTime)
    {
        _test = test;
        _testTime = testTime;
    }

    [HttpGet]
    public IActionResult Get()
    => this.Ok($"{_test.SayHello()}@{_testTime.GetTime()}");
}

public interface ITest
{
    string SayHello();
}

public interface ITestTime
{
    DateTime GetTime();
}

public class TestTime : ITestTime
{
    public DateTime GetTime() => DateTime.Now;
}

public class MyTest : ITest
{
    public string SayHello() => "Hi there";
}

public class MyRegistry : ServiceRegistry
{
    public MyRegistry()
    {
        Scan(s =>
        {
            s.TheCallingAssembly();
            s.WithDefaultConventions();
        });
    }
}