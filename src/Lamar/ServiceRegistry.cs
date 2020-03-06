using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.IoC.Instances;
using Lamar.IoC.Setters;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public static class InstanceExtensions
    {
        public static T Named<T>(this T instance, string name) where T : Instance
        {
            instance.Name = name;
            return instance;
        }

        public static T Scoped<T>(this T instance) where T : Instance
        {
            instance.Lifetime = ServiceLifetime.Scoped;
            return instance;
        }

        public static T Singleton<T>(this T instance) where T : Instance
        {
            instance.Lifetime = ServiceLifetime.Singleton;
            return instance;
        }

        public static T Transient<T>(this T instance) where T : Instance
        {
            instance.Lifetime = ServiceLifetime.Transient;
            return instance;
        }
    }

    public partial class ServiceRegistry : List<ServiceDescriptor>, IServiceCollection
    {
        public static ServiceRegistry For(Action<ServiceRegistry> configuration)
        {
            var registry = new ServiceRegistry();
            configuration(registry);

            return registry;
        }

        public ServiceRegistry()
        {
        }

        public ServiceRegistry(IEnumerable<ServiceDescriptor> descriptors)
        {
            AddRange(descriptors);
        }

        /// <summary>
        /// This method is a shortcut for specifying the default constructor and 
        /// setter arguments for a ImplementationType.  ForConcreteType is shorthand for:
        /// For[T]().Use[T].**************
        /// when the ServiceType and ImplementationType are the same Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BuildWithExpression<T> ForConcreteType<T>() where T : class
        {
            var instance = For<T>().Use<T>();
            return new BuildWithExpression<T>(instance);
        }
        
        /// <summary>
        /// Define the constructor and setter arguments for the default T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class BuildWithExpression<T>
        {
            public BuildWithExpression(ConstructorInstance<T> instance)
            {
                Configure = instance;
            }

            public ConstructorInstance<T> Configure { get; }
        }



        public InstanceExpression<T> For<T>() where T : class
        {
            return new InstanceExpression<T>(this, ServiceLifetime.Transient);
        }

        public DescriptorExpression For(Type serviceType)
        {
             return new DescriptorExpression(serviceType, this);
        }

        public class DescriptorExpression
        {
            private readonly Type _serviceType;
            private readonly ServiceRegistry _parent;

            public DescriptorExpression(Type serviceType, ServiceRegistry parent)
            {
                _serviceType = serviceType;
                _parent = parent;
            }

            public ConstructorInstance Use(Type concreteType)
            {
                var instance = new ConstructorInstance(_serviceType, concreteType, ServiceLifetime.Transient);
                _parent.Add(instance);

                return instance;
            }

            public ConstructorInstance Add(Type implementationType)
            {
                var instance = new ConstructorInstance(_serviceType, implementationType, ServiceLifetime.Transient);
                _parent.Add(instance);
                return instance;
            }

            /// <summary>
            /// Decorate all registrations to the service type with the supplied decorator type
            /// </summary>
            /// <param name="decoratorType"></param>
            public void DecorateAllWith(Type decoratorType)
            {
                var policy = new DecoratorPolicy(_serviceType, decoratorType);
                _parent.Policies.DecorateWith(policy);
            }
        }

        public class InstanceExpression<T> where T : class
        {
            private readonly ServiceRegistry _parent;
            private readonly ServiceLifetime? _lifetime;

            public InstanceExpression(ServiceRegistry parent, ServiceLifetime? lifetime)
            {
                _parent = parent;
                _lifetime = lifetime;
            }



            public ConstructorInstance<TConcrete> Use<TConcrete>() where TConcrete : class, T
            {
                var instance = ConstructorInstance.For<T, TConcrete>();
                if (_lifetime != null) instance.Lifetime = _lifetime.Value;

                _parent.Add(instance);

                return instance;
            }

            /// <summary>
            /// Register a custom instance
            /// </summary>
            /// <param name="instance"></param>
            public void Use(Instance instance)
            {
                if (_lifetime != null)
                {
                    instance.Lifetime = _lifetime.Value;
                }
                
                _parent.Add(instance);
            }

            /// <summary>
            /// Fills in a default type implementation for a service type if there are no prior
            /// registrations
            /// </summary>
            /// <typeparam name="TConcrete"></typeparam>
            /// <exception cref="NotImplementedException"></exception>
            public void UseIfNone<TConcrete>() where TConcrete : class, T
            {
                if (_parent.FindDefault<T>() == null)
                {
                    Use<TConcrete>();
                }
            }

            /// <summary>
            /// Fills in a default type implementation for a service type if there are no prior
            /// registrations
            /// </summary>
            /// <typeparam name="TConcrete"></typeparam>
            public void UseIfNone(T service)
            {
                if (_parent.FindDefault<T>() == null)
                {
                    Use(service);
                }
            }

            /// <summary>
            /// Delegates to Use<TConcrete>(), polyfill for StructureMap syntax
            /// </summary>
            /// <typeparam name="TConcrete"></typeparam>
            /// <exception cref="NotImplementedException"></exception>
            public ConstructorInstance<TConcrete> Add<TConcrete>() where TConcrete : class, T
            {
                return Use<TConcrete>();
            }

            public ObjectInstance Use(T service)
            {
                var instance = new ObjectInstance(typeof(T), service);
                _parent.Add(instance);

                return instance;
            }

            public ObjectInstance Add(T instance)
            {
                return Use(instance);
            }

            public LambdaInstance<IServiceContext, T> Add(Func<IServiceContext, T> func) 
            {
                var instance = LambdaInstance.For(func);
                if (_lifetime != null) instance.Lifetime = _lifetime.Value;

                _parent.Add(instance);

                return instance;

            }
            
            public LambdaInstance<IServiceContext, T> Use(Func<IServiceContext, T> func)
            {
                return Add(func);

            }

            /// <summary>
            /// Decorates all instances of T with the concrete type TDecorator
            /// </summary>
            /// <typeparam name="TDecorator"></typeparam>
            /// <exception cref="NotImplementedException"></exception>
            public void DecorateAllWith<TDecorator>() where TDecorator : T
            {
                var policy = new DecoratorPolicy(typeof(T), typeof(TDecorator));
                _parent.AddSingleton<IDecoratorPolicy>(policy);
            }
        }

        public InstanceExpression<T> ForSingletonOf<T>() where T : class
        {
            return new InstanceExpression<T>(this, ServiceLifetime.Singleton);
        }

        public void Scan(Action<IAssemblyScanner> scan)
        {
            var finder = new AssemblyScanner(this);
            scan(finder);

            finder.Start();

            var descriptor = ServiceDescriptor.Singleton(finder);
            Add(descriptor);
        }

        public void IncludeRegistry<T>() where T : ServiceRegistry, new()
        {
            this.AddRange(new T());
        }

        public void IncludeRegistry<T>(T serviceRegistry) where T : ServiceRegistry
        {
            this.AddRange(serviceRegistry);
        }

        /// <summary>
        /// Configure Container-wide policies and conventions
        /// </summary>
        public PoliciesExpression Policies => new PoliciesExpression(this);

        public class SharingSettings
        {
            public DynamicAssemblySharing Sharing { get; set; }
        }
        
        public class PoliciesExpression
        {
            private readonly ServiceRegistry _parent;


            public PoliciesExpression(ServiceRegistry parent)
            {
                _parent = parent;
            }

            /// <summary>
            /// *If* you have an ill-behaved set of dependencies that somehow manage
            /// to have multiple types with the exact same full name, but in separate
            /// assemblies, you can tell Lamar to build each dynamic in a separate assembly to
            /// get around type references. If you're using mixed ASP.Net Core 2.0 and 2.1 assemblies,
            /// you'll want this
            /// </summary>
            [Obsolete("This will be unnecessary with the 1.1 model where you don't generate singletons upfront")]
            public DynamicAssemblySharing DynamicAssemblySharing
            {
                set => _parent.AddSingleton(new SharingSettings {Sharing = value});
            }

            
            /// <summary>
            /// Adds a new policy to this container
            /// that can apply to every object instance created
            /// by this container
            /// </summary>
            /// <param name="policy"></param>
            public void Add(ILamarPolicy policy)
            {
                if (policy is IInstancePolicy ip) _parent.AddSingleton(ip);
                if (policy is IFamilyPolicy fp) _parent.AddSingleton(fp);
                if (policy is IRegistrationPolicy rp) _parent.AddSingleton(rp);
                if (policy is IDecoratorPolicy dp) _parent.AddSingleton(dp);
                if (policy is ISetterPolicy sp) _parent.AddSingleton(sp);
            }

            /// <summary>
            /// Adds a new policy to this container
            /// that can apply to every object instance created
            /// by this container
            /// </summary>
            public void Add<T>() where T : ILamarPolicy, new()
            {
                Add(new T());
            }


            /// <summary>
            /// Register a strategy for automatically resolving "missing" families
            /// when an unknown PluginType is first encountered
            /// </summary>
            /// <param name="policy"></param>
            public void OnMissingFamily(IFamilyPolicy policy)
            {
                _parent.AddSingleton(policy);
            }

            public void OnMissingFamily<T>() where T : IFamilyPolicy, new()
            {
                OnMissingFamily(new T());
            }

            /// <summary>
            /// Register a strategy for applying decorators on existing registrations
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void DecorateWith<T>() where T : IDecoratorPolicy, new()
            {
                DecorateWith(new T());
            }

            public void DecorateWith(IDecoratorPolicy policy)
            {
                _parent.AddSingleton(policy);
            }

            /// <summary>
            /// Creates automatic "policies" for which public setters are considered mandatory
            /// properties by StructureMap that will be "setter injected" as part of the 
            /// construction process.
            /// </summary>
            /// <param name="action"></param>
            public void SetAllProperties(Action<SetterConvention> action)
            {
                var convention = new SetterConvention(this);
                action(convention);
            }

            /// <summary>
            /// Directs StructureMap to always inject dependencies into any and all public Setter properties
            /// of the type TPluginType.
            /// </summary>
            /// <typeparam name="TServiceType"></typeparam>
            /// <returns></returns>
            public InstanceExpression<TServiceType> FillAllPropertiesOfType<TServiceType>() where TServiceType : class
            {
                Add(new LambdaSetterPolicy(prop => prop.PropertyType == typeof(TServiceType)));

                return _parent.For<TServiceType>();
            }
        }

        /// <summary>
        /// Tells Lamar that the service "T" will be injected into a built
        /// container later. Used for framework support
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Injectable<T>() where T : class
        {
            For<T>().Use(new InjectedInstance<T>());
        }

        internal T[] FindAndRemovePolicies<T>() where T : ILamarPolicy
        {
            var policies = this
                .Where(x => x.ServiceType == typeof(T) && x.ImplementationInstance != null)
                .Select(x => x.ImplementationInstance)
                .OfType<T>()
                .ToArray();

            RemoveAll(x => x.ServiceType == typeof(T));

            return policies;
        }

        public InverseInstanceExpression<T> Use<T>() where T : class
        {
            return new InverseInstanceExpression<T>(this);
        }
    }

    public enum DynamicAssemblySharing
    {
        Shared,
        Individual
    }
}
