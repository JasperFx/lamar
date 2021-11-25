using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// use Lamar as DI.
builder.Host.UseLamar();
builder.Host.ConfigureContainer<ServiceRegistry>(services =>
{
    // register services using Lamar
    services.For<ITest>().Use<MyTest>();
    services.IncludeRegistry<MyRegistry>();
    
    // add the controllers
    services.AddControllers();
});


var app = builder.Build();
app.MapControllers();
app.Run();

[Route("api/[controller]")]
[ApiController]
public class HelloController : ControllerBase
{
    private readonly ITest test;
    private readonly ITestTime testTime;

    public HelloController(ITest test, ITestTime testTime)
    {
        this.test = test;
        this.testTime = testTime;
    }

    [HttpGet]
    public IActionResult Get()
    => this.Ok($"{this.test.SayHello()}@{this.testTime.GetTime()}");
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