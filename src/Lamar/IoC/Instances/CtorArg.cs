using System.Reflection;
using Lamar.IoC.Frames;
using LamarCodeGeneration.Model;

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
            return variables.Resolve(Instance, mode);
        }
    }
}