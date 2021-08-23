using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.IoC.Activation;
using Lamar.IoC.Instances;
using Lamar.IoC.Setters;
using Lamar.Scanning.Conventions;
using LamarCodeGeneration;
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
            assertNotObjectInstance(instance);
            instance.Lifetime = ServiceLifetime.Scoped;
            return instance;
        }

        private static void assertNotObjectInstance<T>(T instance) where T : Instance
        {
            if (instance is ObjectInstance)
                throw new InvalidOperationException(
                    "You cannot override the lifecycle of a direct object registration. Did you mean to use a Lambda registration instead?");
        }

        public static T Singleton<T>(this T instance) where T : Instance
        {
            instance.Lifetime = ServiceLifetime.Singleton;
            return instance;
        }

        public static T Transient<T>(this T instance) where T : Instance
        {
            assertNotObjectInstance(instance);
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
            public BuildWithExpression(ConstructorInstance<T, T> instance)
            {
                Configure = instance;
            }

            public ConstructorInstance<T, T> Configure { get; }
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
            /// Register a custom Instance
            /// </summary>
            /// <param name="instance"></param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public void Use(Instance instance)
            {
                if (instance.ServiceType != _serviceType) 
                    throw new ArgumentOutOfRangeException(nameof(instance), $"The Instance.ServiceType {instance.ServiceType.FullNameInCode()} was not {_serviceType.FullNameInCode()}");
                
                _parent.Add(instance);
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



            public ConstructorInstance<TConcrete, T> Use<TConcrete>() where TConcrete : class, T
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
            /// <typeparam name="T"></typeparam>
            public void UseIfNone(T service)
            {
                if (_parent.FindDefault<T>() == null)
                {
                    Use(service);
                }
            }

            /// <summary>
            /// Delegates to Use&lt;T&gt;(), polyfill for StructureMap syntax
            /// </summary>
            /// <typeparam name="TConcrete"></typeparam>
            public ConstructorInstance<TConcrete, T> Add<TConcrete>() where TConcrete : class, T
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
            public void DecorateAllWith<TDecorator>() where TDecorator : T
            {
                var policy = new DecoratorPolicy(typeof(T), typeof(TDecorator));
                _parent.AddSingleton<IDecoratorPolicy>(policy);
            }

            /// <summary>
            /// Intercept the object being created and potentially replace it with a wrapped
            /// version or another object. This will apply to every registration where the service
            /// type is T or the implementation type could be cast to T
            /// </summary>
            /// <param name="interceptor"></param>
            /// <returns></returns>
            public void InterceptAll(Func<T, T> interceptor)
            {
                InterceptAll((s, x) => interceptor(x));
            }
        
            /// <summary>
            /// Intercept the object being created and potentially replace it with a wrapped
            /// version or another object. This will apply to every registration where the service
            /// type is T or the implementation type could be cast to T
            /// </summary>
            /// <param name="interceptor"></param>
            /// <returns></returns>
            public void InterceptAll(Func<IServiceContext, T, T> interceptor)
            {
                var policy = new InterceptorPolicy<T>(interceptor);
                _parent.Policies.Add(policy);
            }
        
            /// <summary>
            /// Perform some action on the object being created at the time the object is created for the first time by Lamar.
            /// This will apply to every registration where the service
            /// type is T or the implementation type could be cast to T
            /// </summary>
            /// <param name="activator"></param>
            /// <returns></returns>
            public void OnCreationForAll(Action<IServiceContext, T> activator)
            {
                var policy = new ActivationPolicy<T>(activator);
                _parent.Policies.Add(policy);
            }
        
            /// <summary>
            /// Perform some action on the object being created at the time the object is created for the first time by Lamar.
            /// This will apply to every registration where the service
            /// type is T or the implementation type could be cast to T
            /// </summary>
            /// <param name="activator"></param>
            /// <returns></returns>
            public void OnCreationForAll(Action<T> activator)
            {
                OnCreationForAll((c, x) => activator(x));
            }
        }

        /// <summary>
        /// Shorthand equivalent to `For<T>().******.Singleton()`
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public InstanceExpression<T> ForSingletonOf<T>() where T : class
        {
            return new InstanceExpression<T>(this, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Create an isolated type scanning registration policy 
        /// </summary>
        /// <param name="scan"></param>
        public void Scan(Action<IAssemblyScanner> scan)
        {
            var finder = new AssemblyScanner(this);
            scan(finder);

            finder.Start();

            var descriptor = ServiceDescriptor.Singleton(finder);
            Add(descriptor);
        }

        internal IList<Type> RegistryTypes { get; set; } = new List<Type>();

        /// <summary>
        /// Include the registrations from another ServiceRegistry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void IncludeRegistry<T>() where T : ServiceRegistry, new()
        {
            IncludeRegistry(new T());
        }

        /// <summary>
        /// Include the registrations from another ServiceRegistry
        /// </summary>
        /// <param name="serviceRegistry"></param>
        /// <typeparam name="T"></typeparam>
        public void IncludeRegistry<T>(T serviceRegistry) where T : ServiceRegistry
        {
            Include(serviceRegistry);
        }

        /// <summary>
        /// Include the registrations from another ServiceRegistry
        /// </summary>
        /// <param name="registry"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Include(ServiceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            var type = registry.GetType();
            if (type != typeof(ServiceRegistry))
            {
                if (RegistryTypes.Contains(type)) return;
                
                this.AddRange(registry);
                RegistryTypes.Add(type);
            }
            else
            {
                this.AddRange(registry);
            }
        }

        /// <summary>
        /// Configure Container-wide policies and conventions
        /// </summary>
        public PoliciesExpression Policies => new PoliciesExpression(this);

        public class PoliciesExpression
        {
            private readonly ServiceRegistry _parent;


            internal PoliciesExpression(ServiceRegistry parent)
            {
                _parent = parent;
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
            /// when an unknown ServiceType is first encountered
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
            /// of the type TServiceType.
            /// </summary>
            /// <typeparam name="TType"></typeparam>
            /// <returns></returns>
            public InstanceExpression<TType> FillAllPropertiesOfType<TType>() where TType : class
            {
                Add(new LambdaSetterPolicy(prop => prop.PropertyType == typeof(TType)));

                return _parent.For<TType>();
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

        public ProvidedInstanceInverseInstanceExpression<T> Use<T>(T instance) where T : class
        {
            return new ProvidedInstanceInverseInstanceExpression<T>(this, instance);
        }
    }

    public enum DynamicAssemblySharing
    {
        Shared,
        Individual
    }
}
