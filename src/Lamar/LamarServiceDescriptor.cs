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
///     The subclass routes the Instance through Lamar-specific state, while
///     delegating to the (Type, Type, ServiceLifetime) base constructor so the
///     descriptor's surface-area Lifetime is honest. Lamar's own self-discovery
///     in <see cref="Instance.FindServiceRegistration" /> (and the related
///     Matches helper) prefers the typed Instance when present and otherwise
///     reconstructs from the descriptor's ImplementationType / Lifetime — the
///     two paths produce equivalent results for simple registrations.
/// </summary>
public sealed class LamarServiceDescriptor : ServiceDescriptor
{
    public LamarServiceDescriptor(Instance instance)
        : base(instance.ServiceType, instance.ImplementationType ?? instance.ServiceType, instance.Lifetime)
    {
        Instance = instance;
    }

    public Instance Instance { get; }
}
