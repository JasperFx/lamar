using System;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar;

/// <summary>
///     Makes Lamar treat a Type as a singleton in the lifecycle scoping
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ScopedAttribute : LamarAttribute
{
    // This method will affect single registrations
    public override void Alter(IConfiguredInstance instance)
    {
        instance.Lifetime = ServiceLifetime.Scoped;
    }
}