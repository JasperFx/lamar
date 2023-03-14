using System.Linq;
using Lamar.IoC;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;
using Lamar.Testing.IoC.Acceptance;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC;

public class ServiceGraphTester
{
    public readonly ServiceRegistry theServices = new();

    [Fact]
    public void no_family_for_assembly_scanner_or_policy_or_connected_concretions()
    {
        var graph = ServiceGraph.For(_ =>
        {
            _.Scan(x =>
            {
                x.TheCallingAssembly();
                x.ConnectImplementationsToTypesClosing(typeof(type_scanning.IGeneric<>));
            });

            _.Policies.OnMissingFamily<CustomMissingFamily>();
        });

        graph.HasFamily(typeof(ConnectedConcretions)).ShouldBeFalse();
        graph.HasFamily(typeof(IFamilyPolicy)).ShouldBeFalse();
        graph.HasFamily(typeof(AssemblyScanner)).ShouldBeFalse();
    }

    [Fact]
    public void finds_the_single_default()
    {
        theServices.AddTransient<IWidget, AWidget>();
        var theGraph = new ServiceGraph(theServices, Scope.Empty());
        theGraph.Initialize();

        theGraph.FindDefault(typeof(IWidget))
            .ShouldBeOfType<ConstructorInstance>()
            .ImplementationType.ShouldBe(typeof(AWidget));
    }

    [Fact]
    public void finds_the_last_as_the_default()
    {
        theServices.AddTransient<IWidget, AWidget>();
        theServices.AddSingleton(this);
        theServices.AddTransient<IThing, Thing>();
        theServices.AddTransient<IWidget, MoneyWidget>();

        var theGraph = new ServiceGraph(theServices, Scope.Empty());
        theGraph.Initialize();

        theGraph.FindDefault(typeof(IWidget))
            .ShouldBeOfType<ConstructorInstance>()
            .ImplementationType.ShouldBe(typeof(MoneyWidget));
    }

    [Fact]
    public void finds_all()
    {
        theServices.AddTransient<IWidget, AWidget>();
        theServices.AddSingleton(this);
        theServices.AddTransient<IThing, Thing>();
        theServices.AddTransient<IWidget, MoneyWidget>();

        var theGraph = new ServiceGraph(theServices, Scope.Empty());
        theGraph.Initialize();

        theGraph.FindAll(typeof(IWidget))
            .OfType<ConstructorInstance>()
            .Select(x => x.ImplementationType)
            .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(MoneyWidget));
    }

    [Fact]
    public void register_an_instance_that_is_not_the_default()
    {
        var instance = ObjectInstance.For(new Clock());
        var graph = ServiceGraph.For(_ =>
        {
            _.Add(instance);
            _.AddSingleton(new Clock());
        });


        graph.FindInstance(instance.ServiceType, instance.Name)
            .ShouldBeSameAs(instance);
    }

    [Fact]
    public void register_an_instance_that_is_the_default()
    {
        var instance = ObjectInstance.For(new Clock());
        var graph = ServiceGraph.For(_ => { _.Add(instance); });


        graph.FindInstance(instance.ServiceType, instance.Name)
            .ShouldBeSameAs(instance);
    }
}