using System.Threading.Tasks;
using Lamar.Microsoft.DependencyInjection;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton;

[assembly:OaktonCommandAssembly]

namespace GeneratorTarget
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return Host.CreateDefaultBuilder()
                .UseLamar((c, services) =>
                {
                    services.AddSingleton<IGeneratesCode>(new GreeterGenerator());
                    services.AddSingleton<IGeneratesCode>(new GreeterGenerator2());
                })
                .RunOaktonCommands(args);

        }
    }
}