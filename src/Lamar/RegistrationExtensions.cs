using System;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Apply service overrides to Lamar that will take priority regardless of the service ordering
        /// otherwise. This is primarily meant for test automation scenarios
        /// </summary>
        /// <param name="services"></param>
        /// <param name="overrides"></param>
        public static void OverrideServices(this IServiceCollection services, Action<ServiceRegistry> overrides)
        {
            var overrideRegistry = new LamarOverrides();
            overrides(overrideRegistry.Overrides);

            services.AddSingleton<IRegistrationPolicy>(overrideRegistry);
        }
    }
}