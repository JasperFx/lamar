using System.Reflection;
using Lamar.IoC.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public class CtorArg
    {
        public ParameterInfo Parameter { get; }
        public Instance Instance { get; }

        public CtorArg(ParameterInfo parameter, Instance instance)
        {
            Parameter = parameter;
            Instance = instance;
            
            if (instance.IsInlineDependency() || instance is LambdaInstance && instance.ServiceType.IsGenericType)
            {
                instance.Name = Parameter.Name;
            }
        }

        public Variable Resolve(ResolverVariables variables, BuildMode mode)
        {
            var variable = variables.Resolve(Instance, mode);
            
            if (Parameter.Name.EqualsIgnoreCase(variable.Usage))
            {
                variable.OverrideName("inline_" + variable.Usage);
            }

            return variable;
        }
    }
}