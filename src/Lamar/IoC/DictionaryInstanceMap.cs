using System.Collections.Concurrent;

namespace Lamar.IoC;

public class DictionaryInstanceMap : InstanceMap
{
    private readonly ConcurrentDictionary<InstanceIdentifier, object> _dictionary = new();
    
    public override bool TryFind(InstanceIdentifier key, out object value) => _dictionary.TryGetValue(key, out value);

    public override InstanceMap AddOrUpdate(InstanceIdentifier hash, object value)
    {
        _dictionary.AddOrUpdate(hash, _ => value, (_, _) => value);

        return this;
    }
}