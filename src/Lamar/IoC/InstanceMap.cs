using System;

namespace Lamar.IoC;

public abstract class InstanceMap
{
    public static Behavior DefaultBehavior { get; set; } = Enum.TryParse<Behavior>(Environment.GetEnvironmentVariable("LAMAR_INSTANCEMAPBEHAVIOR"), out var instanceMapBehavior)
        ? instanceMapBehavior
        : Behavior.HashMap;
    
    public abstract bool TryFind(InstanceIdentifier key, out object value);

    public abstract InstanceMap AddOrUpdate(InstanceIdentifier hash, object value);
    
    public enum Behavior
    {
        HashMap,
        Dictionary
    }
}