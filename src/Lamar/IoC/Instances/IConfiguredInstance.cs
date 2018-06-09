using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    // SAMPLE: IConfiguredInstance
    public interface IConfiguredInstance
    {
        /// <summary>
        /// The constructor function that this registration is going to use to
        /// construct the object
        /// </summary>
        ConstructorInfo Constructor { get; set; }
        
        /// <summary>
        /// The service type that you can request. This would normally be an interface or other
        /// abstraction
        /// </summary>
        Type ServiceType { get; }
        
        /// <summary>
        /// The actual, concrete type
        /// </summary>
        Type ImplementationType { get; }
        
        
        ServiceLifetime Lifetime { get; set; }
        
        /// <summary>
        /// The instance name for requesting this object by name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Inline definition of a constructor dependency.  Select the constructor argument by type and constructor name.
        ///     Use this method if there is more than one constructor arguments of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructorArg"></param>
        /// <returns></returns>
        DependencyExpression<T> Ctor<T>(string constructorArg = null);
        
        /// <summary>
        /// Directly add or interrogate the inline dependencies for this instance
        /// </summary>
        IReadOnlyList<Instance> InlineDependencies { get; }

        /// <summary>
        /// Adds an inline dependency
        /// </summary>
        /// <param name="instance"></param>
        void AddInline(Instance instance);
    }
    // ENDSAMPLE
}