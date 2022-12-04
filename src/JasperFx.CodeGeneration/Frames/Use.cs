using System;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;

namespace JasperFx.CodeGeneration.Frames;

/// <summary>
///     Stand in for a Variable top be resolved later
/// </summary>
public class Use
{
    private readonly string _variableName;

    private readonly Type _variableType;

    public Use(Type variableType)
    {
        _variableType = variableType;
    }

    public Use(Type variableType, string variableName)
    {
        _variableType = variableType;
        _variableName = variableName;
    }

    internal Variable FindVariable(IMethodVariables variables)
    {
        return _variableName.IsNotEmpty()
            ? variables.FindVariableByName(_variableType, _variableName)
            : variables.FindVariable(_variableType);
    }

    public static Use Type<T>(string variableName = null)
    {
        return new Use(typeof(T), variableName);
    }
}