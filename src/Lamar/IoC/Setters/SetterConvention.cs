using System;
using System.Reflection;
using JasperFx.Core.Reflection;
using JasperFx.CodeGeneration.Util;

namespace Lamar.IoC.Setters
{
    /// <summary>
    /// Used as an expression builder to specify setter injection policies
    /// </summary>
    public class SetterConvention
    {
        private readonly ServiceRegistry.PoliciesExpression _parent;

        public SetterConvention(ServiceRegistry.PoliciesExpression parent)
        {
            _parent = parent;
        }


        /// <summary>
        /// Directs StructureMap to treat all public setters of type T as
        /// mandatory properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void OfType<T>()
        {
            Matching(prop => prop.PropertyType == typeof (T));
        }

        /// <summary>
        /// Directs StructureMap to tread all public setters with
        /// a PropertyType that matches the predicate as a
        /// mandatory setter
        /// </summary>
        /// <param name="predicate"></param>
        public void TypeMatches(Predicate<Type> predicate)
        {
            Matching(prop => predicate(prop.PropertyType));
        }

        /// <summary>
        /// Directs StructureMap to treat all public setters that match the 
        /// rule as mandatory properties
        /// </summary>
        /// <param name="rule"></param>
        public void Matching(Func<PropertyInfo, bool> rule)
        {
            _parent.Add(new LambdaSetterPolicy(rule));
        }

        /// <summary>
        /// Directs StructureMap to treat all public setters with a property
        /// type in the specified namespace as mandatory properties
        /// </summary>
        /// <param name="nameSpace"></param>
        public void WithAnyTypeFromNamespace(string nameSpace)
        {
            Matching(prop => prop.PropertyType.IsInNamespace(nameSpace));
        }

        /// <summary>
        /// Directs StructureMap to treat all public setters with a property
        /// type in the specified namespace as mandatory properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithAnyTypeFromNamespaceContainingType<T>()
        {
            WithAnyTypeFromNamespace(typeof (T).Namespace);
        }

        /// <summary>
        /// Directs StructureMap to treat all public setters where to property name
        /// matches the specified rule as a mandatory property
        /// </summary>
        /// <param name="rule"></param>
        public void NameMatches(Predicate<string> rule)
        {
            Matching(prop => rule(prop.Name));
        }


    }
}