using System;
using System.Linq;
using Lamar.IoC.Instances;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lamar.Microsoft.DependencyInjection
{
    
    
    
    public class LoggerPolicy : IRegistrationPolicy, IFamilyPolicy
    {
        public static bool Matches(ServiceDescriptor descriptor)
        {
            return  descriptor.ServiceType.Closes(typeof(ILogger<>))
                    && !descriptor.ServiceType.IsOpenGeneric()
                    && descriptor.ImplementationType.Closes(typeof(Logger<>));
        }
        
        public void Apply(ServiceRegistry registry)
        {
            for (var index = registry.Count - 1; index >= 0; --index)
            {
                if (!Matches(registry[index])) continue;
                
                var type = registry[index].ServiceType.FindParameterTypeTo(typeof(ILogger<>));
                var instance = typeof(LoggerInstance<>).CloseAndBuildAs<Instance>(type);
                registry[index] = instance.ToDescriptor();


            }
        }

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.Closes(typeof(ILogger<>)))
            {
                var argType = type.GetGenericArguments().Single();
                var instance = typeof(LoggerInstance<>).CloseAndBuildAs<Instance>(argType);
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, instance);
            }

            return null;
        }
    }
}