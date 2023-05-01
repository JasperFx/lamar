using System;
using System.Reflection;

namespace Lamar.IoC.Setters;

/// <summary>
///     Setter policy using a lambda test
/// </summary>
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