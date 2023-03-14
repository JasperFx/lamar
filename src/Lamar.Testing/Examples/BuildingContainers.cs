using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.Widget;

namespace Lamar.Testing.Examples;

public class BuildingContainers
{
    public void dostuff()
    {
        // Idiomatic StructureMap
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<AWidget>().Named("A");

            // StructureMap's old type scanning
            _.Scan(s =>
            {
                s.TheCallingAssembly();
                s.WithDefaultConventions();
            });
        });

        var widget = container.GetInstance<IWidget>();

        // ASP.Net Core DI compatible
        IServiceProvider container2 = new Container(_ =>
        {
            _.AddTransient<IWidget, AWidget>();
            _.AddSingleton(new MoneyWidget());
        });

        var widget2 = container.GetService<IWidget>();
    }
}