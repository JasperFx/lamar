using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Lamar.Util;

namespace Lamar.IoC.Setters
{

    public class InjectedSetter
    {
        public PropertyInfo Property { get; }
        public Instance Instance { get; }

        public InjectedSetter(PropertyInfo property, Instance instance)
        {
            Property = property;
            Instance = instance;
        }


        public void ApplyQuickBuildProperties(object service, Scope scope)
        {
            var value = Instance.QuickResolve(scope);
            Property.SetValue(service, value);
        }

        public SetterArg Resolve(ResolverVariables variables, BuildMode mode)
        {
            Variable variable;
            if (Instance.IsInlineDependency())
            {
                variable = Instance.CreateInlineVariable(variables);

                // HOKEY. Might need some smarter way of doing this. Helps to disambiguate
                // between ctor args of nested decorators
                if (!(variable is Setter))
                {
                    variable.OverrideName(variable.Usage + "_inline_" + ++variables.VariableSequence);
                }
            }
            else
            {
                variable = variables.Resolve(Instance, mode);
            }
                
            return new SetterArg(variable, Property);
        }
    }

    public class SetterArg
    {
        public Variable Variable { get; }
        public PropertyInfo Property { get; }

        public SetterArg(Variable variable, PropertyInfo property)
        {
            Variable = variable;
            Property = property;
        }

        public string InlineAssignment => $"{Property.Name} = {Variable.Usage}";

        public string ToSetPropertyCode(ServiceVariable target)
        {
            return $"{target.Usage}.{Property.Name} = {Variable.Usage}";
        }
    }
    

    public interface ISetterPolicy : ILamarPolicy
    {
        bool Matches(PropertyInfo prop);
    }

    public class LambdaSetterPolicy : ISetterPolicy
    {
        private readonly Func<PropertyInfo, bool> _match;

        public LambdaSetterPolicy(Func<PropertyInfo, bool> match)
        {
            _match = match;
        }

        public bool Matches(PropertyInfo prop)
        {
            return _match(prop);
        }
    }


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