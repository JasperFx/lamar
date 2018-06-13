using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Baseline;
using Lamar;
using Lamar.Codegen;
using Lamar.IoC.Instances;
using Lamar.Microsoft.DependencyInjection;
using Lamar.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.AspNetCore;

public class BenchmarkBase : IDisposable
{
    protected IWebHost _blueMilkHost;


    public BenchmarkBase()
    {
        var builder = new WebHostBuilder();
        builder
            .UseLamar()
            .ConfigureServices(services =>
            {
                var registry = new ServiceRegistry();
                configure(registry);

                services.AddRange(registry);
            })
            .UseUrls("http://localhost:5002")
            .UseServer(new NulloServer())
            .UseStartup<Startup>();

        _blueMilkHost = builder.Start();

        Instances = _blueMilkHost.Services.As<Container>().Model.AllInstances
            .Where(x => x.ServiceType.Assembly != typeof(Container).Assembly && !x.ServiceType.IsOpenGeneric())
            .Where(x => x.ServiceType != typeof(IServiceProviderFactory<ServiceRegistry>))
            .Select(x => x.Instance)
            .ToArray();

        Types = Instances.Select(x => x.ServiceType).Distinct().ToArray();


        singletons = Instances.Where(x => x.Lifetime == ServiceLifetime.Singleton).Select(x => x.ServiceType)
            .Distinct().ToArray();

        scoped = Instances.Where(x => x.Lifetime == ServiceLifetime.Scoped).Select(x => x.ServiceType)
            .Distinct().ToArray();

        transients = Instances.Where(x => x.Lifetime == ServiceLifetime.Transient && x.ServiceType.IsPublic).Select(x => x.ServiceType)
            .Distinct().ToArray();

        objects = Instances.OfType<ObjectInstance>().Select(x => x.ServiceType).Distinct().ToArray();

        lambdas = Instances.OfType<LambdaInstance>().Select(x => x.ServiceType).Distinct().ToArray();

        internals = Instances.Where(x => x.ImplementationType.IsNotPublic).Select(x => x.ServiceType).Distinct().ToArray();
    }

    public Type[] internals { get; set; }


    public Type[] lambdas { get; set; }

    public Type[] objects { get; set; }

    public Type[] transients { get; set; }

    public Type[] scoped { get; set; }

    public Type[] singletons { get; set; }

    public Instance[] Instances { get; }

    protected virtual void configure(ServiceRegistry services)
    {
        
    }

    public IContainer Lamar => _blueMilkHost.Services.As<IContainer>();

    public Type[] Types { get; }

    public virtual void Dispose()
    {
        _blueMilkHost?.Dispose();

    }


}

