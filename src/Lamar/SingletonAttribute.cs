using System;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    // SAMPLE: SingletonAttribute
    /// <summary>
    /// Makes Lamar treat a Type as a singleton in the lifecycle scoping
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SingletonAttribute : LamarAttribute
    {
        // This method will affect single registrations
        public override void Alter(IConfiguredInstance instance)
        {
            instance.Lifetime = ServiceLifetime.Singleton;
        }
    }
    // ENDSAMPLE
}