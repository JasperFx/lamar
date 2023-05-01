using System.Reflection;

namespace Lamar.IoC.Setters;

/// <summary>
///     Establishes a test of whether a property should be "settable" in
///     object construction
/// </summary>
public interface ISetterPolicy : ILamarPolicy
{
    bool Matches(PropertyInfo prop);
}