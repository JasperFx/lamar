using Lamar;
using Lamar.Microsoft.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Host.UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory());
builder.Host.ConfigureContainer<ServiceRegistry>(builder =>
{
    builder.For<ITest>().Use<MyTest>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

public interface ITest
{
    string SayHello();
}

public class MyTest : ITest
{
    public string SayHello() => "Hi there";
}
