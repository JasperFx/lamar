using System;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.AspNetCore;

namespace Benchmarks
{
    public abstract class ComparisonBenchmark : BenchmarkBase
    {
        private IWebHost _smHost;
        private IWebHost _aspnetHost;
        
        protected ComparisonBenchmark()
        {
            var builder2 = new WebHostBuilder();
            builder2
                .UseUrls("http://localhost:5002")
                .ConfigureServices(services =>
                {
                    services.AddMvc();
                    services.AddLogging();
                })
                .UseServer(new NulloServer())
                .UseStartup<Startup2>()
                .UseStructureMap();

            _smHost = builder2.Start();


            var builder3 = new WebHostBuilder();
            builder3
                .UseUrls("http://localhost:5002")
                .ConfigureServices(services =>
                {
                    services.AddMvc();
                    services.AddLogging();
                })
                .UseServer(new NulloServer())
                .UseStartup<Startup3>();

            _aspnetHost = builder3.Start();
        }


        [Params("Lamar", "StructureMap", "AspNetCore")]
        public string ProviderName { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            _smHost?.Dispose();
            _aspnetHost?.Dispose();
        }
        
        public IServiceProvider StructureMap => _smHost.Services;
        public IServiceProvider AspNetCore => _aspnetHost.Services;
        
        public IServiceProvider For(string name)
        {
            switch (name)
            {
                case "Lamar":
                    return Lamar;
                
                case "StructureMap":
                    return StructureMap;
                
                case "AspNetCore":
                    return AspNetCore;
            }
        
            throw new ArgumentOutOfRangeException();
        }


        protected void buildAll(Type[] types)
        {
            var provider = For(ProviderName);

            foreach (var type in types)
            {
                var value = provider.GetService(type);
            }
        }
    }
}