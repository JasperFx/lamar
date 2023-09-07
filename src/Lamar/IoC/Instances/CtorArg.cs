using System;
using System.Reflection;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.IoC.Frames;

namespace Lamar.IoC.Instances;

public class CtorArg
{
    public CtorArg(ParameterInfo parameter, Instance instance)
    {
        try
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));

            if (instance.IsInlineDependency() || (instance is LambdaInstance && instance.ServiceType.IsGenericType))
            {
                instance.Name = Parameter.Name;
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Cannot create a Constructor Argument for {parameter?.Name ?? "anonymous"} of type {parameter.ParameterType.FullNameInCode()} of {instance}. If this is a primitive type like strings or numbers, Lamar will not do any automatic resolution by type", e);
        }
    }

    public ParameterInfo Parameter { get; }
    public Instance Instance { get; }

    public Variable Resolve(ResolverVariables variables, BuildMode mode)
    {
        var variable = variables.Resolve(Instance, mode);

        if (Parameter.Name.IsNotEmpty() && Parameter.Name.EqualsIgnoreCase(variable.Usage))
        {
            variable.OverrideName("inline_" + variable.Usage);
        }

        return variable;
    }
}