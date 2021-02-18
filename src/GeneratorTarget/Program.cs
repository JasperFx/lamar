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
                    services.AddSingleton<IGeneratesCode>(new ConsoleWriterGenerator("Writer1",
                        "Hello from writer 1", "Writer1"));
                    
                    services.AddSingleton<IGeneratesCode>(new ConsoleWriterGenerator("Writer2",
                        "Hello from writer 2", "Writer2"));
                    
                    services.AddSingleton<IGeneratesCode>(new ConsoleWriterGenerator("Writer3",
                        "Hello from writer 3", "Writer3"));
                })
                .RunOaktonCommands(args);

        }
    }
}