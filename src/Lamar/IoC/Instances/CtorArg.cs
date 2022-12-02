using System;
using System.Reflection;
using JasperFx.Core;
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
            try
            {
                Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
                Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            
                if (instance.IsInlineDependency() || instance is LambdaInstance && instance.ServiceType.IsGenericType)
                {
                    instance.Name = Parameter.Name;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Cannot create a Constructor Argument for {parameter.Name} of {instance}", e);
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