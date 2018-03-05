using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using StructureMap;

public class Startup2
{
    public void ConfigureContainer(Registry services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Run(c => c.Response.WriteAsync("Hello"));
    }
}