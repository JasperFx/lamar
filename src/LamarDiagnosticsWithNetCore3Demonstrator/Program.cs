using System;
using System.Threading.Tasks;
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
                .UseLamar()
                .RunOaktonCommands(args);
        }
    }
}