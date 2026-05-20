using System;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar;

/// <summary>
///     ServiceDescriptor variant that carries a Lamar Instance alongside its
///     intended ServiceLifetime. This solves a long-standing mismatch: the
///     stock ServiceDescriptor(Type, object) constructor — which is the only
///     MS-DI overload that accepts a pre-existing object payload — hard-codes
///     Lifetime = Singleton, forcing every fluent registration (For<T>().Use<TImpl>(),
///     .Use(factory), .Add<TImpl>(), and their .Scoped()/.Transient() variants)
///     to appear as a Singleton to anything inspecting the IServiceCollection
///     snapshot. That's the root cause behind Wolverine HTTP code-gen capturing
///     resolved Lamar registrations as singleton fields on generated handler
///     classes; see JasperFx/wolverine#1610 and #537 for visible symptoms.
///
///     Descriptor shape: we delegate to the (Type, Func, ServiceLifetime) base
///     constructor, which makes the descriptor look like an "opaque lambda
///     factory" to external consumers. This is the shape Wolverine's docs say
///     triggers service-location at runtime — exactly what we want, because
///     service location calls back into Lamar's IServiceProvider, which can
///     see scan-only deps (whereas Wolverine's inline-construction codegen
///     cannot). The factory delegate itself is never invoked under Lamar
///     resolution: Lamar's <see cref="Instance.For" /> checks for the subclass
///     first and returns the typed Instance directly, bypassing the factory
///     branch. The factory body throws to make any incorrect direct invocation
///     loud and obvious.
/// </summary>
public sealed class LamarServiceDescriptor : ServiceDescriptor
{
    public LamarServiceDescriptor(Instance instance)
        : base(
            instance.ServiceType,
            FactoryShouldNotBeInvoked,
            instance.Lifetime)
    {
        Instance = instance;
    }

    public Instance Instance { get; }

    private static object FactoryShouldNotBeInvoked(IServiceProvider sp) =>
        throw new InvalidOperationException(
            "LamarServiceDescriptor's ImplementationFactory is a Wolverine/MS-DI codegen marker " +
            "and should not be invoked directly. Resolution under Lamar goes through Instance.For " +
            "(which prefers the typed Instance on the subclass) and never through this delegate. " +
            "If you see this, you are resolving the descriptor outside a Lamar container — build " +
            "the container via UseLamar(...) instead of stock MS-DI ServiceProvider.");
}
