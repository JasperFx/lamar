using System.Reflection;

namespace Lamar.Scanning.Conventions;

public static class AssemblyLoader
{
    public static Assembly ByName(string assemblyName)
    {
        return Assembly.Load(new AssemblyName(assemblyName));
    }
}