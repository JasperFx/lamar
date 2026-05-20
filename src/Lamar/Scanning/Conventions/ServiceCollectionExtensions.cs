using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions;

[JasperFxIgnore]
internal class ConnectedConcretions : List<Type>
{
}

internal static class ServiceCollectionExtensions
{
    public static bool HasScanners(this IEnumerable<ServiceDescriptor> services)
    {
        return services.Any(x => x.ServiceType == typeof(AssemblyScanner));
    }

    public static ConnectedConcretions ConnectedConcretions(this IServiceCollection services)
    {
        var concretions = services.FirstOrDefault(x => x.ServiceType == typeof(ConnectedConcretions))
            ?.ImplementationInstance as ConnectedConcretions;

        if (concretions == null)
        {
            concretions = new ConnectedConcretions();
            services.AddSingleton(concretions);
        }

        return concretions;
    }

    /// <summary>
    ///     Add a registration via Lamar's intrinsic Instance type.
    ///
    ///     Lamar smuggles its rich Instance metadata through ServiceDescriptor by
    ///     placing the Instance object in the ImplementationInstance slot. The
    ///     built-in MS-DI ServiceDescriptor(Type, object) constructor forces
    ///     Lifetime = Singleton, which lies to any IServiceCollection consumer
    ///     (Wolverine code-gen in particular) that reads descriptor.Lifetime to
    ///     decide how to source the dependency. We use a LamarServiceDescriptor
    ///     subclass so the descriptor carries the Lamar Instance AND the intended
    ///     lifetime; Lamar's own self-discovery treats it the same as a legacy
    ///     ImplementationInstance round-trip via the helper below.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="instance"></param>
    public static void Add(this IServiceCollection services, Instance instance)
    {
        var descriptor = new LamarServiceDescriptor(instance);
        instance.ServiceCollection = services;
        instance.LamarDescriptor = descriptor;
        services.Add(descriptor);
    }

    /// <summary>
    ///     Called by <see cref="Instance.Lifetime" />'s setter when the lifetime changes.
    ///     Locates the Instance's existing LamarServiceDescriptor in the registry by
    ///     reference and replaces it with a fresh one reflecting current Instance state.
    ///     Matches by descriptor reference (not by ServiceType, which is what MS-DI's
    ///     own <c>IServiceCollection.Replace</c> extension does — wrong semantics for us
    ///     when multiple registrations share a ServiceType, e.g. <c>For&lt;T&gt;().Add&lt;&gt;()</c>
    ///     pipelines).
    /// </summary>
    internal static void ReplaceLamarDescriptor(this Instance instance)
    {
        var services = instance.ServiceCollection;
        var current = instance.LamarDescriptor;
        if (services == null || current == null) return;

        var idx = services.IndexOf(current);
        if (idx < 0) return;

        var refreshed = new LamarServiceDescriptor(instance);
        services[idx] = refreshed;
        instance.LamarDescriptor = refreshed;
    }

    /// <summary>
    ///     Returns the Lamar Instance attached to <paramref name="descriptor" />, if any —
    ///     covering both the LamarServiceDescriptor subclass and the legacy
    ///     ImplementationInstance round-trip shape.
    /// </summary>
    internal static Instance LamarInstance(this ServiceDescriptor descriptor)
    {
        if (descriptor is LamarServiceDescriptor lsd) return lsd.Instance;
        return descriptor.ImplementationInstance as Instance;
    }

    public static bool Matches(this ServiceDescriptor descriptor, Type serviceType, Type implementationType)
    {
        if (descriptor.ServiceType != serviceType)
        {
            return false;
        }

        if (descriptor.ImplementationType == implementationType)
        {
            return true;
        }

        var lamarInstance = descriptor.LamarInstance();
        if (lamarInstance != null)
        {
            return lamarInstance.ImplementationType == implementationType;
        }

        return false;
    }

    public static Instance AddType(this IServiceCollection services, Type serviceType, Type implementationType,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var hasAlready = services.Any(x => x.Matches(serviceType, implementationType));
        if (!hasAlready)
        {
            var instance = new ConstructorInstance(serviceType, implementationType, lifetime);

            services.Add(instance);

            return instance;
        }

        return null;
    }

    public static ServiceDescriptor FindDefault<T>(this IServiceCollection services)
    {
        return services.FindDefault(typeof(T));
    }

    public static ServiceDescriptor FindDefault(this IServiceCollection services, Type serviceType)
    {
        return services.LastOrDefault(x => x.ServiceType == serviceType);
    }
}