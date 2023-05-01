using JasperFx.CodeGeneration.Model;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Frames;

public static class VariableExtensions
{
    public static bool RefersTo(this Variable variable, Instance instance)
    {
        return instance == (variable as IServiceVariable)?.Instance;
    }
}