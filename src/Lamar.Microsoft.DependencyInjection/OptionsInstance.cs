using System;
using System.Linq;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LamarCodeGeneration.Util;

namespace Lamar.Microsoft.DependencyInjection
{
    public class OptionsPolicy : IRegistrationPolicy, IFamilyPolicy
    {
        public static bool Matches(ServiceDescriptor descriptor)
        {
                return  descriptor.ServiceType.Closes(typeof(IOptions<>))
                    && !descriptor.ServiceType.IsOpenGeneric()
                    && descriptor.ImplementationType.Closes(typeof(OptionsManager<>));
        }
        
        public void Apply(ServiceRegistry registry)
        {
            for (var index = registry.Count - 1; index >= 0; --index)
            {
                if (!Matches(registry[index])) continue;

                var openType = registry[index].ServiceType.Closes(typeof(IOptionsSnapshot<>))
                    ? typeof(OptionsSnapshotInstance<>)
                    : typeof(OptionsInstance<>);
                
                var type = registry[index].ServiceType.FindParameterTypeTo(typeof(IOptions<>));
                var instance = openType.CloseAndBuildAs<Instance>(type);
                registry[index] = instance.ToDescriptor();


            }
        }

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.Closes(typeof(IOptionsSnapshot<>)))
            {
                var argType = type.GetGenericArguments().Single();
                var instance = typeof(OptionsSnapshotInstance<>).CloseAndBuildAs<Instance>(argType);
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, instance);
            }
            
            if (type.Closes(typeof(IOptions<>)))
            {
                var argType = type.GetGenericArguments().Single();
                var instance = typeof(OptionsInstance<>).CloseAndBuildAs<Instance>(argType);
                return new ServiceFamily(type, serviceGraph.DecoratorPolicies, instance);
            }

            return null;
        }
    }   
    
    public class OptionsSnapshotInstance<T> : Instance where T : class, new()
    {
        public OptionsSnapshotInstance() : base(typeof(IOptionsSnapshot<T>), typeof(OptionsManager<T>), ServiceLifetime.Singleton)
        {
        }
        
        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => resolveFromRoot(topScope);
        }

        public override object Resolve(Scope scope)
        {
            return resolveFromRoot(scope.Root);
        }
        
        private readonly object _locker = new object();

        private object resolveFromRoot(Scope root)
        {
            if (tryGetService(root, out object service))
            {
                return service;
            }

            lock (_locker)
            {
                if (tryGetService(root, out service))
                {
                    return service;
                }
                
                var setups = root.QuickBuildAll<IConfigureOptions<T>>();
                var postConfigures = root.QuickBuildAll<IPostConfigureOptions<T>>();

                var options = new OptionsManager<T>(new OptionsFactory<T>(setups, postConfigures));
            
            
                store(root, options);

                return options;
            }


        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new InjectedServiceField(this);
        }
    }
    
    public class OptionsInstance<T> : Instance where T : class, new()
    {
        public OptionsInstance() : base(typeof(IOptions<T>), typeof(OptionsManager<T>), ServiceLifetime.Singleton)
        {
        }
        
        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            return s => resolveFromRoot(topScope);
        }

        public override object Resolve(Scope scope)
        {
            return resolveFromRoot(scope.Root);
        }
        
        private readonly object _locker = new object();

        private object resolveFromRoot(Scope root)
        {
            if (tryGetService(root, out object service))
            {
                return service;
            }

            lock (_locker)
            {
                if (tryGetService(root, out service))
                {
                    return service;
                }
                
                var setups = root.QuickBuildAll<IConfigureOptions<T>>();
                var postConfigures = root.QuickBuildAll<IPostConfigureOptions<T>>();

                var options = new OptionsManager<T>(new OptionsFactory<T>(setups, postConfigures));
            
            
                store(root, options);

                return options;
            }


        }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            return new InjectedServiceField(this);
        }
    }
}