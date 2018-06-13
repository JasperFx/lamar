using System;
using Lamar.IoC;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    /// <summary>
    /// A diagnostic wrapper around registered Instance's 
    /// </summary>
    public class InstanceRef
    {
        private readonly Scope _rootScope;
        private readonly Scope _scope;

        public InstanceRef(Instance instance, Scope scope)
        {
            Instance = instance;
            _scope = scope;
            _rootScope = scope.Root;
        }

        /// <summary>
        /// The underlying Lamar model for building this configured Instance. ACCESS THIS WITH CAUTION!
        /// </summary>
        public Instance Instance { get; }

        /// <summary>
        /// The lifecycle of this specific Instance
        /// </summary>
        public ServiceLifetime Lifetime => Instance.Lifetime;

        public string Name => Instance.Name;

        /// <summary>
        ///     The actual concrete type of this Instance.  Not every type of IInstance
        ///     can determine the ConcreteType
        /// </summary>
        public Type ImplementationType => Instance.ImplementationType;



        public Type ServiceType => Instance.ServiceType;


        /// <summary>
        /// Returns the real object represented by this Instance
        /// resolved by the underlying Container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : class
        {
            return Resolve() as T;
        }

        /// <summary>
        /// Has the object already been created and 
        /// cached in its Lifetime?  Mostly useful
        /// for Singleton's
        /// </summary>
        /// <returns></returns>
        public bool ObjectHasBeenCreated()
        {
            switch (Lifetime)
            {
                case ServiceLifetime.Transient:
                    return false;
                case ServiceLifetime.Scoped:
                    return _scope.Services.ContainsKey(Instance.Hash);
                case ServiceLifetime.Singleton:
                    return _scope.Root.Services.ContainsKey(Instance.Hash);
            }

            return false;
        }

        /// <summary>
        /// Creates the textual representation of the 'BuildPlan'
        /// for this Instance
        /// </summary>
        /// <returns></returns>
        public string DescribeBuildPlan()
        {
            return Instance.GetBuildPlan(_rootScope);
        }

        /// <summary>
        /// Builds or resolves an object instance for this registration
        /// </summary>
        /// <returns></returns>
        public object Resolve()
        {
            return Instance.Resolve(_scope);
        }

        public override string ToString()
        {
            return Instance.ToString();
        }
    }
}