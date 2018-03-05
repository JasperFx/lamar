using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class Startup3
{
    public void Configure(IApplicationBuilder app)
    {
        app.Run(c => c.Response.WriteAsync("Hello"));
    }
}