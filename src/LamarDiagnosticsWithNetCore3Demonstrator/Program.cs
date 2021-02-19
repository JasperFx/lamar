using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lamar;
using Lamar.Diagnostics;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;

namespace LamarDiagnosticsWithNetCore3Demonstrator
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return new HostBuilder()
                .UseLamar((context, services) =>
                {
                    services.CheckLamarConfiguration();
                    services.Scan(s =>
                    {
                        s.TheCallingAssembly();
                        s.WithDefaultConventions();
                    });
                    
                    services.For<IEngine>().Use<Hemi>().Named("The Hemi");

                    services.For<IEngine>().Add<VEight>().Singleton().Named("V8");
                    services.For<IEngine>().Add<FourFiftyFour>();
                    services.For<IEngine>().Add<StraightSix>().Scoped();

                    services.For<IEngine>().Add(c => new Rotary()).Named("Rotary");
                    services.For<IEngine>().Add(c => c.GetService<PluginElectric>());

                    services.For<IEngine>().Add(new InlineFour());

                    services.For<IWidget>().Use<AWidget>();

                    services.For<AWidget>().Use<AWidget>();

                    services.ForConcreteType<DeepConstructorGuy>();

                    services.ForConcreteType<EngineChoice>();
                    
                    services.For<IService>().Use(new ColorService("red"));

                    services.Policies.SetAllProperties(policy => { policy.TypeMatches(type => type == typeof(IService)); });
                    
                    services.For<IGateway>().Use<DefaultGateway>()
                        .Setter<string>("Name").Is("Blue")
                        .Setter<string>("Color").Is("Green");
                })
                .RunOaktonCommands(args);
        }
    }

}