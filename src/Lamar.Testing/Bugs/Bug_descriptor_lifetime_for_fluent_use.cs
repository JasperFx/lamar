using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

// Repro for a bridge issue between Lamar's fluent registration API and the
// IServiceCollection snapshot Lamar exposes (since ServiceRegistry : IServiceCollection).
//
// When a Lamar Instance is added to the registry via the extension at
// src/Lamar/Scanning/Conventions/ServiceCollectionExtensions.cs:42-45:
//
//     public static void Add(this IServiceCollection services, Instance instance)
//     {
//         services.Add(new ServiceDescriptor(instance.ServiceType, instance));
//     }
//
// the descriptor constructor used is ServiceDescriptor(Type, object) which
// hard-codes ServiceLifetime.Singleton (it's the only MS-DI overload that
// accepts a pre-existing instance). So every For<T>().Use<TImpl>() /
// For<T>().Use(factory) / For<T>().Add<TImpl>() / ...Scoped() / ...Transient()
// call writes a ServiceDescriptor whose Lifetime is Singleton, regardless of
// what the underlying Lamar Instance says.
//
// Lamar itself still honors Instance.Lifetime when resolving, so apps that
// only ever go through Container.GetInstance<T>() never see this. But anything
// that reads the IServiceCollection snapshot directly (e.g. Wolverine 5+'s
// HTTP endpoint code-gen, JasperFx's ServiceCollectionServerVariableSource)
// sees Singleton and treats the registration accordingly. That's the
// "Use existing HttpContext ServiceProvider for opaque scoped services"
// scenario tracked at JasperFx/wolverine#1610 — for users of Lamar via
// Wolverine HTTP it manifests as captive dependencies: a single resolved
// instance baked into a singleton-lifetime generated handler class.
//
// This file demonstrates the underlying mismatch as plain xUnit tests against
// Lamar alone, no Wolverine reference needed.
public interface ILifetimeBridgeThing { }
public class LifetimeBridgeThing : ILifetimeBridgeThing { }

public class Bug_descriptor_lifetime_for_fluent_use
{
    // Convention-scan registrations (WithDefaultConventions()) are NOT affected:
    // DefaultConventionScanner.ScanTypes calls the (serviceType, implType, lifetime)
    // descriptor ctor directly at DefaultConventionScanner.cs:28, so the descriptor
    // carries an honest lifetime. The bug below is specific to the fluent API paths
    // that route through the Add(IServiceCollection, Instance) extension.

    // ------- Fluent paths: descriptor.Lifetime is silently Singleton ----------

    [Fact]
    public void For_Use_writes_Transient_lifetime_to_descriptor()
    {
        // ServiceRegistry.For<T>() returns an InstanceExpression with
        // ServiceLifetime.Transient as its default (ServiceRegistry.cs:96).
        // The Instance Lamar adds also has Lifetime = Transient. But the
        // ServiceDescriptor surfaced to IServiceCollection consumers will
        // be Singleton.
        var registry = new ServiceRegistry();
        registry.For<ILifetimeBridgeThing>().Use<LifetimeBridgeThing>();

        var descriptor = registry.LastOrDefault(d => d.ServiceType == typeof(ILifetimeBridgeThing));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void For_Use_Scoped_writes_Scoped_lifetime_to_descriptor()
    {
        // .Scoped() updates instance.Lifetime to Scoped (see
        // InstanceExtensions.Scoped in ServiceRegistry.cs:23). The descriptor
        // still ends up Singleton.
        var registry = new ServiceRegistry();
        registry.For<ILifetimeBridgeThing>().Use<LifetimeBridgeThing>().Scoped();

        var descriptor = registry.LastOrDefault(d => d.ServiceType == typeof(ILifetimeBridgeThing));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void For_Use_factory_Scoped_writes_Scoped_lifetime_to_descriptor()
    {
        // The lambda-factory overload — same bug. This is the exact shape
        // that AMI uses to register IAsyncDocumentSession against RavenDB
        // and that triggers captive sessions in Wolverine HTTP handlers.
        var registry = new ServiceRegistry();
        registry.For<ILifetimeBridgeThing>().Use(_ => new LifetimeBridgeThing()).Scoped();

        var descriptor = registry.LastOrDefault(d => d.ServiceType == typeof(ILifetimeBridgeThing));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void For_Add_writes_Transient_lifetime_to_descriptor()
    {
        // The multi-impl pipeline form. Same bridge path.
        var registry = new ServiceRegistry();
        registry.For<ILifetimeBridgeThing>().Add<LifetimeBridgeThing>();

        var descriptor = registry.LastOrDefault(d => d.ServiceType == typeof(ILifetimeBridgeThing));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    // ------- Sanity: container resolution honours intent (the consolation) ---

    [Fact]
    public void Container_resolution_still_honours_intended_lifetime()
    {
        // Lamar's own container plan uses Instance.Lifetime correctly, so this
        // passes today. Documenting it to make clear the bug is purely at the
        // IServiceCollection-snapshot layer.
        var container = new Container(x => x.For<ILifetimeBridgeThing>().Use<LifetimeBridgeThing>().Scoped());

        using var scope1 = container.GetNestedContainer();
        using var scope2 = container.GetNestedContainer();
        var a1 = scope1.GetInstance<ILifetimeBridgeThing>();
        var a2 = scope1.GetInstance<ILifetimeBridgeThing>();
        var b1 = scope2.GetInstance<ILifetimeBridgeThing>();

        a1.ShouldBeSameAs(a2);   // same scope -> same instance
        a1.ShouldNotBeSameAs(b1); // different scope -> different instance
    }
}
