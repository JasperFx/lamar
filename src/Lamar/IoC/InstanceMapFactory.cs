using System;

namespace Lamar.IoC;

public static class InstanceMapFactory
{
    public static InstanceMap Get(InstanceMap.Behavior instanceMapBehavior)
    {
        return instanceMapBehavior switch
        {
            InstanceMap.Behavior.Dictionary => new DictionaryInstanceMap(),
            InstanceMap.Behavior.HashMap => new HashMapInstanceMap(),
            _ => throw new ArgumentOutOfRangeException(nameof(instanceMapBehavior), instanceMapBehavior, null)
        };
    }
}