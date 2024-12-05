using System;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Resolvers;

public class ScopeResolver : IResolver
{
    public object Resolve(Scope scope)
    {
        return scope;
    }

    public Type ServiceType { get; } = typeof(Scope);
    public string Name { get; set; } = "default";
    public InstanceIdentifier Hash { get; set; } = new ("default", typeof(Scope));
}