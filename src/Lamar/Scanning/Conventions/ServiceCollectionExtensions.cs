using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    [LamarIgnore]
    public class ConnectedConcretions : List<Type>
    {
    }

    public static class ServiceCollectionExtensions
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
        ///     Add a registration via Lamar's intrinsic Instance type
        /// </summary>
        /// <param name="services"></param>
        /// <param name="instance"></param>
        public static void Add(this IServiceCollection services, Instance instance)
        {
            services.Add(new ServiceDescriptor(instance.ServiceType, instance));
        }

        public static bool Matches(this ServiceDescriptor descriptor, Type serviceType, Type implementationType)
        {
            if (descriptor.ServiceType != serviceType) return false;

            if (descriptor.ImplementationType == implementationType) return true;

            if (descriptor.ImplementationInstance is Instance)
                return descriptor.ImplementationInstance.As<Instance>().ImplementationType == implementationType;

            return false;
        }

        public static Instance AddType(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
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

        public static Type[] RegisteredTypesFor<T>(this IServiceCollection services)
        {
            return services
                .Where(x => x.ServiceType == typeof(T) && x.ImplementationType != null)
                .Select(x => x.ImplementationType)
                .ToArray();
        }
    }
}