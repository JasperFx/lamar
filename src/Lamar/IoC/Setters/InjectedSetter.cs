using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Lamar.IoC.Setters
{

    internal class InjectedSetter
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
                
            return new SetterArg(Property, variable);
        }
    }
}