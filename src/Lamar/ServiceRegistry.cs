using System;
using System.Collections.Generic;
using Lamar.IoC.Instances;
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

    public class ServiceRegistry : List<ServiceDescriptor>, IServiceCollection
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
        }

        public class InstanceExpression<T> where T : class
        {
            private readonly ServiceRegistry _parent;
            private readonly ServiceLifetime _lifetime;

            public InstanceExpression(ServiceRegistry parent, ServiceLifetime lifetime)
            {
                _parent = parent;
                _lifetime = lifetime;
            }



            public ConstructorInstance<TConcrete> Use<TConcrete>() where TConcrete : class, T
            {
                var instance = ConstructorInstance.For<T, TConcrete>();
                instance.Lifetime = _lifetime;

                _parent.Add(instance);

                return instance;
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

                _parent.Add(instance);

                return instance;

            }
            
            public LambdaInstance<IServiceContext, T> Use(Func<IServiceContext, T> func)
            {
                return Add(func);

            }

        }

        public InstanceExpression<T> ForSingletonOf<T>() where T : class
        {
            return new InstanceExpression<T>(this, ServiceLifetime.Singleton);
        }

        public void Scan(Action<IAssemblyScanner> scan)
        {
            var finder = new AssemblyScanner();
            scan(finder);

            finder.Start();

            var descriptor = ServiceDescriptor.Singleton(finder);
            Add(descriptor);
        }


        public void IncludeRegistry<T>() where T : ServiceRegistry, new()
        {
            this.AddRange(new T());
        }

        /// <summary>
        /// Configure Container-wide policies and conventions
        /// </summary>
        public PoliciesExpression Policies => new PoliciesExpression(this);

        public class PoliciesExpression
        {
            private readonly ServiceRegistry _parent;


            public PoliciesExpression(ServiceRegistry parent)
            {
                _parent = parent;
            }

            
            /// <summary>
            /// Adds a new instance policy to this container
            /// that can apply to every object instance created
            /// by this container
            /// </summary>
            /// <param name="policy"></param>
            public void Add(IInstancePolicy policy)
            {
                _parent.AddSingleton(policy);
            }

            /// <summary>
            /// Adds a new instance policy to this container
            /// that can apply to every object instance created
            /// by this container
            /// </summary>
            public void Add<T>() where T : IInstancePolicy, new()
            {
                Add(new T());
            }
/*
            /// <summary>
            /// Register an interception policy
            /// </summary>
            /// <param name="policy"></param>
            public void Interceptors(IInterceptorPolicy policy)
            {
                alter = graph => graph.Policies.Add(policy);
            }
*/


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


        }

    }
}
