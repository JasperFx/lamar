using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using LamarCodeGeneration.Util;


namespace Lamar.IoC.Instances
{
    public abstract class Instance
    {
        public abstract Func<Scope, object> ToResolver(Scope topScope);
        
        public bool IsOnlyOneOfServiceType { get; set; }

        public ServiceDescriptor ToDescriptor()
        {
            return new ServiceDescriptor(ServiceType, this);
        }
        
        public string DefaultArgName()
        {
            var argName = Variable.DefaultArgName(ServiceType);

            if (ServiceType.IsGenericType)
            {
                argName += "_of_" + ServiceType.GetGenericArguments().Select(t => t.NameInCode().Sanitize()).Join("_");
            }
            
            
            return IsOnlyOneOfServiceType ? argName : argName + HashCode(ServiceType, Name).ToString().Replace("-", "_");
        }

        internal IEnumerable<Assembly> ReferencedAssemblies()
        {
            yield return ServiceType.Assembly;
            yield return ImplementationType.Assembly;

            if (ServiceType.IsGenericType)
            {
                foreach (var type in ServiceType.GetGenericArguments())
                {
                    yield return type.Assembly;
                }
            }

            if (ImplementationType.IsGenericType)
            {
                foreach (var type in ImplementationType.GetGenericArguments())
                {
                    yield return type.Assembly;
                }
            }
        }


        public Type ServiceType { get; }
        public Type ImplementationType { get; }

        public static Instance For(ServiceDescriptor service)
        {
            if (service.ImplementationInstance is Instance instance) return instance;

            if (service.ImplementationInstance != null) return new ObjectInstance(service.ServiceType, service.ImplementationInstance);

            if (service.ImplementationFactory != null) return new LambdaInstance(service.ServiceType, service.ImplementationFactory, service.Lifetime);

            return new ConstructorInstance(service.ServiceType, service.ImplementationType, service.Lifetime);
        }

        public static bool CanBeCastTo(Type implementationType, Type serviceType)
        {
            if (implementationType == null) return false;

            if (implementationType == serviceType) return true;


            if (serviceType.IsOpenGeneric())
            {
                return GenericsPluginGraph.CanBeCast(serviceType, implementationType);
            }

            if (implementationType.IsOpenGeneric())
            {
                return false;
            }


            return serviceType.GetTypeInfo().IsAssignableFrom(implementationType.GetTypeInfo());
        }

        protected Instance(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (!CanBeCastTo(implementationType, serviceType))
            {
                throw new ArgumentOutOfRangeException(nameof(implementationType), $"{implementationType.FullNameInCode()} cannot be cast to {serviceType.FullNameInCode()}");
            }

            ServiceType = serviceType;
            Lifetime = lifetime;
            ImplementationType = implementationType;
            Name = "default";

        }

        public abstract object Resolve(Scope scope);

        /// <summary>
        /// Resolve the service as if it were only going to ever be resolved once
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public virtual object QuickResolve(Scope scope)
        {
            return Resolve(scope);
        }

        public int Hash { get; set; }

        public virtual bool RequiresServiceProvider => Dependencies.Any(x => x.RequiresServiceProvider);

        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Hash = GetHashCode();
            }
        }

        public bool HasPlanned { get; protected internal set; }

        public void CreatePlan(ServiceGraph services)
        {
            if (HasPlanned) return;

            try
            {
                services.StartingToPlan(this);
            }
            catch (Exception e)
            {
                ErrorMessages.Add(e.Message);

                services.FinishedPlanning();
                HasPlanned = true;
                return;
            }

            // Can't do the planning on open generic types 'cause bad stuff happens
            if (!ServiceType.IsOpenGeneric())
            {
                foreach (var policy in services.InstancePolicies)
                {
                    policy.Apply(this);
                }

                var atts = ImplementationType.GetAllAttributes<LamarAttribute>();
                foreach (var att in atts)
                {
                    att.Alter(this);
                    if (this is IConfiguredInstance)
                    {
                        att.Alter(this.As<IConfiguredInstance>());
                    }
                }
                
                var dependencies = createPlan(services) ?? Enumerable.Empty<Instance>();

                if (dependencies.Any(x => x.Dependencies.Contains(this)))
                {
                    throw new InvalidOperationException("Bi-directional dependencies detected to " + ToString());
                }

                Dependencies = dependencies.Concat(dependencies.SelectMany(x => x.Dependencies)).Distinct().ToArray();
            }

            services.ClearPlanning();
            HasPlanned = true;
        }


        public abstract Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot);

        public virtual Variable CreateInlineVariable(ResolverVariables variables)
        {
            return CreateVariable(BuildMode.Dependency, variables, false);
        }
        
        protected virtual IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            return Enumerable.Empty<Instance>();
        }

        public readonly IList<string> ErrorMessages = new List<string>();
        private string _name = "default";


        public Instance[] Dependencies { get; protected set; } = new Instance[0];

        /// <summary>
        /// Is this instance known to be dependent upon the dependencyType?
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <returns></returns>
        public bool DependsOn(Type dependencyType)
        {
            return Dependencies.Any(x => x.ServiceType == dependencyType || x.ImplementationType == dependencyType);
        }

        public bool IsDefault { get; set; } = false;

        protected bool tryGetService(Scope scope, out object service)
        {
            return scope.Services.TryGetValue(Hash, out service);
        }

        protected void store(Scope scope, object service)
        {
            scope.Services.Add(Hash, service);
        }

        /// <summary>
        /// Tries to describe how this instance would be resolved at runtime
        /// </summary>
        /// <param name="rootScope"></param>
        internal virtual string GetBuildPlan(Scope rootScope) => ToString();


        public sealed override int GetHashCode()
        {
            unchecked
            {
                return HashCode(ServiceType, Name);
            }
        }

        public static int HashCode(Type serviceType, string name = null)
        {
            return (serviceType.GetHashCode() * 397) ^ (name ?? "default").GetHashCode();
        }

        public virtual Instance CloseType(Type serviceType, Type[] templateTypes)
        {
            return null;
        }
        
        /// <summary>
        /// Only used to track naming within inline dependencies
        /// </summary>
        internal Instance Parent { get; set; }

        public bool IsInlineDependency()
        {
            return Parent != null;
        }

        protected string inlineSetterName()
        {
            var name = Name;
            var parent = Parent;
            while (parent != null)
            {
                name = parent.Name + "_" + name;
                parent = parent.Parent;
            }

            return "func_" + name.Sanitize();
        }
    }
}
