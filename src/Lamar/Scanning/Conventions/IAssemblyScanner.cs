using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Lamar.Scanning.Conventions
{
    public interface IAssemblyScanner
    {
        /// <summary>
        ///     Optional user-supplied diagnostic description of this scanning operation
        /// </summary>
        string Description { get; set; }

        /// <summary>
        ///     Add an Assembly to the scanning operation
        /// </summary>
        /// <param name="assembly"></param>
        void Assembly(Assembly assembly);

        /// <summary>
        ///     Add an Assembly by name to the scanning operation
        /// </summary>
        /// <param name="assemblyName"></param>
        void Assembly(string assemblyName);

        /// <summary>
        ///     Add the Assembly that contains type T to the scanning operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AssemblyContainingType<T>();

        /// <summary>
        ///     Add the Assembly that contains type to the scanning operation
        /// </summary>
        /// <param name="type"></param>
        void AssemblyContainingType(Type type);

        /// <summary>
        ///     Add all concrete types of the Plugin Type as Instances of Plugin Type
        /// </summary>
        /// <typeparam name="TPluginType"></typeparam>
        FindAllTypesFilter AddAllTypesOf<TPluginType>();

        /// <summary>
        ///     Add all concrete types of the Plugin Type as Instances of Plugin Type
        ///     with the specified lifetime
        /// </summary>
        /// <typeparam name="TPluginType"></typeparam>
        /// <param name="lifetime"></param>
        FindAllTypesFilter AddAllTypesOf<TPluginType>(ServiceLifetime lifetime);

        /// <summary>
        ///     Add all concrete types of the Plugin Type as Instances of Plugin Type
        /// </summary>
        /// <param name="pluginType"></param>
        FindAllTypesFilter AddAllTypesOf(Type pluginType);

        /// <summary>
        ///     Add all concrete types of the Plugin Type as Instances of Plugin Type
        ///     with the specified lifetime
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="lifetime"></param>
        FindAllTypesFilter AddAllTypesOf(Type pluginType, ServiceLifetime lifetime);

        /// <summary>
        ///     Exclude types that match the Predicate from being scanned
        /// </summary>
        /// <param name="exclude"></param>
        void Exclude(Func<Type, bool> exclude);

        /// <summary>
        ///     Exclude all types in this nameSpace or its children from the scanning operation
        /// </summary>
        /// <param name="nameSpace"></param>
        void ExcludeNamespace(string nameSpace);

        /// <summary>
        ///     Exclude all types in this nameSpace or its children from the scanning operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ExcludeNamespaceContainingType<T>();

        /// <summary>
        ///     Only include types matching the Predicate in the scanning operation. You can
        ///     use multiple Include() calls in a single scanning operation
        /// </summary>
        /// <param name="predicate"></param>
        void Include(Func<Type, bool> predicate);

        /// <summary>
        ///     Only include types from this nameSpace or its children in the scanning operation.  You can
        ///     use multiple Include() calls in a single scanning operation
        /// </summary>
        /// <param name="nameSpace"></param>
        void IncludeNamespace(string nameSpace);

        /// <summary>
        ///     Only include types from this nameSpace or its children in the scanning operation.  You can
        ///     use multiple Include() calls in a single scanning operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void IncludeNamespaceContainingType<T>();

        /// <summary>
        ///     Exclude this specific type from the scanning operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ExcludeType<T>();

        /// <summary>
        ///     Adds a registration convention to be applied to all the types in this
        ///     logical "scan" operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void Convention<T>() where T : IRegistrationConvention, new();

        /// <summary>
        ///     Adds a registration convention to be applied to all the types in this
        ///     logical "scan" operation
        /// </summary>
        void With(IRegistrationConvention convention);

        /// <summary>
        ///     Adds the DefaultConventionScanner to the scanning operations.  I.e., a concrete
        ///     class named "Something" that implements "ISomething" will be automatically
        ///     added to PluginType "ISomething"
        /// </summary>
        void WithDefaultConventions();

        /// <summary>
        ///     Adds the DefaultConventionScanner to the scanning operations with the specified lifetime.
        ///     I.e., a concrete class named "Something" that implements "ISomething" will be automatically
        ///     added to PluginType "ISomething"
        /// </summary>
        /// <param name="lifetime"></param>
        void WithDefaultConventions(ServiceLifetime lifetime);

        /// <summary>
        ///     Adds the DefaultConventionScanner to the scanning operations.  I.e., a concrete
        ///     class named "Something" that implements "ISomething" will be automatically
        ///     added to PluginType "ISomething"
        /// </summary>
        /// <param name="behavior">Define whether or not Lamar should overwrite any existing registrations. Default is IfNew</param>
        void WithDefaultConventions(OverwriteBehavior behavior);

        /// <summary>
        ///     Adds the DefaultConventionScanner to the scanning operations with the specified lifetime.
        ///     I.e., a concrete class named "Something" that implements "ISomething" will be automatically
        ///     added to PluginType "ISomething"
        /// </summary>
        /// <param name="behavior">Define whether or not Lamar should overwrite any existing registrations. Default is IfNew</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> to use</param>
        void WithDefaultConventions(OverwriteBehavior behavior, ServiceLifetime lifetime);

        /// <summary>
        ///     Automatically registers all concrete types without primitive arguments
        ///     against its first interface, if any
        /// </summary>
        void RegisterConcreteTypesAgainstTheFirstInterface();

        /// <summary>
        ///     Automatically registers all concrete types without primitive arguments
        ///     against its first interface, if any, using the specified lifetime
        /// </summary>
        /// <param name="lifetime"></param>
        void RegisterConcreteTypesAgainstTheFirstInterface(ServiceLifetime lifetime);

        /// <summary>
        ///     Directs the scanning to automatically register any type that is the single
        ///     implementation of an interface against that interface.
        ///     The filters apply
        /// </summary>
        void SingleImplementationsOfInterface();

        /// <summary>
        ///     Directs the scanning to automatically register any type that is the single
        ///     implementation of an interface against that interface, using the specified lifetime.
        ///     The filters apply
        /// </summary>
        /// <param name="lifetime"></param>
        void SingleImplementationsOfInterface(ServiceLifetime lifetime);

        void TheCallingAssembly();
        void AssembliesFromApplicationBaseDirectory();
        void AssembliesFromApplicationBaseDirectory(Func<Assembly, bool> assemblyFilter);


        /// <summary>
        ///     Scans for PluginType's and Concrete Types that close the given open generic type
        /// </summary>
        /// <example>
        /// </example>
        /// <param name="openGenericType"></param>
        void ConnectImplementationsToTypesClosing(Type openGenericType);

        /// <summary>
        ///     Scans for PluginType's and Concrete Types that close the given open generic type
        /// </summary>
        /// <example>
        /// </example>
        /// <param name="openGenericType"></param>
        /// <param name="lifetime"></param>
        void ConnectImplementationsToTypesClosing(Type openGenericType, ServiceLifetime lifetime);

        /// <summary>
        ///     Choosing option will direct StructureMap to *also* scan files ending in '*.exe'
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="assemblyFilter"></param>
        /// <param name="includeExeFiles"></param>
        void AssembliesAndExecutablesFromApplicationBaseDirectory(Func<Assembly, bool> assemblyFilter = null);

        void AssembliesAndExecutablesFromPath(string path);
        void AssembliesFromPath(string path);

        void AssembliesAndExecutablesFromPath(string path,
            Func<Assembly, bool> assemblyFilter);

        void AssembliesFromPath(string path,
            Func<Assembly, bool> assemblyFilter);


        /// <summary>
        ///     Directs the scanning operation to automatically detect and include any Registry
        ///     classes found in the Assembly's being scanned
        /// </summary>
        void LookForRegistries();
    }
}