using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    public interface IConfiguredInstance
    {
        ConstructorInfo Constructor { get; set; }
        Type ServiceType { get; }
        Type ImplementationType { get; }
        ServiceLifetime Lifetime { get; set; }
        string Name { get; set; }
        bool IsDefault { get; }

        /// <summary>
        ///     Inline definition of a constructor dependency.  Select the constructor argument by type and constructor name.
        ///     Use this method if there is more than one constructor arguments of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructorArg"></param>
        /// <returns></returns>
        DependencyExpression<T> Ctor<T>(string constructorArg = null);
        
        
        IList<Instance> InlineDependencies { get; }

        bool IsInlineDependency();
    }
}