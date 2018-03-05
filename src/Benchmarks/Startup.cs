using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureContainer(ServiceRegistry services)
    {
        services.AddMvc();
        services.AddLogging();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Run(c => c.Response.WriteAsync("Hello"));
    }
}