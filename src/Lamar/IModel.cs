using System;
using System.Collections.Generic;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;

namespace Lamar
{
    /// <summary>
    /// Can be used to analyze and query the registrations and configuration
    /// of the running Container or Scope
    /// </summary>
    public interface IModel
    {
        /// <summary>
        ///     Retrieves the configuration for the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IServiceFamilyConfiguration For<T>();

        /// <summary>
        ///     Retrieves the configuration for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IServiceFamilyConfiguration For(Type type);

        /// <summary>
        ///     Access to all the <seealso cref="IServiceFamilyConfiguration">Service Type</seealso> registrations
        /// </summary>
        IEnumerable<IServiceFamilyConfiguration> ServiceTypes { get; }

        /// <summary>
        ///     Queryable access to all of the <see cref="InstanceRef">Instance</see> for a given ServiceType
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        IEnumerable<Instance> InstancesOf(Type serviceType);

        /// <summary>
        ///     Queryable access to all of the <see cref="Instance">Instance</see> for a given ServiceType
        /// </summary>
        /// <returns></returns>
        IEnumerable<Instance> InstancesOf<T>();

        /// <summary>
        ///     Find the concrete type for the default Instance of T.
        ///     In other words, when I call Container.GetInstance(Type),
        ///     what do I get?  May be indeterminate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Type DefaultTypeFor<T>();

        /// <summary>
        ///     Find the concrete type for the default Instance of pluginType.
        ///     In other words, when I call Container.GetInstance(Type),
        ///     what do I get?  May be indeterminate
        /// </summary>
        /// <returns></returns>
        Type DefaultTypeFor(Type serviceType);
        
        /// <summary>
        ///     All explicitly known Instance's in this container.  Other instances can be created during
        ///     the lifetime of the container
        /// </summary>
        IEnumerable<Instance> AllInstances { get; }

        /// <summary>
        ///     Get each and every configured instance that could possibly
        ///     be cast to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] GetAllPossible<T>() where T : class;
        
        /// <summary>
        ///     Can Lamar fulfill a request to ObjectFactory.GetInstance(pluginType) from the
        ///     current configuration.  This does not include concrete classes that could be auto-configured
        ///     upon demand
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        bool HasRegistrationFor(Type serviceType);

        /// <summary>
        ///     Can Lamar fulfill a request to ObjectFactory.GetInstance&lt;T&gt;() from the
        ///     current configuration.  This does not include concrete classes that could be auto-configured
        ///     upon demand
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasRegistrationFor<T>();

        /// <summary>
        /// All of the assembly scanning operations that were used to build this
        /// Container
        /// </summary>
        IEnumerable<AssemblyScanner> Scanners { get; }

    }
}