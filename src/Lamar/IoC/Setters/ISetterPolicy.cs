using System.Reflection;

namespace Lamar.IoC.Setters
{
    public interface ISetterPolicy : ILamarPolicy
    {
        bool Matches(PropertyInfo prop);
    }
}