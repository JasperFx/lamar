using System;
using System.Threading.Tasks;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;

namespace LamarDiagnosticsWithNetCore3Demonstrator
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return new HostBuilder()
                .UseLamar((context, services) =>
                {
                    services.Scan(s =>
                    {
                        s.TheCallingAssembly();
                        s.WithDefaultConventions();
                    });
                })
                .RunOaktonCommands(args);
        }
    }
    
    public interface ISetter
    {

    }

    public class Setter : ISetter
    {

    }

    public class SetterHolder
    {
        [SetterProperty]
        public ISetter Setter { get; set; }
    }
}