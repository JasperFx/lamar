using System.Collections.Concurrent;
using JasperFx.Core;

namespace Lamar.IoC;

public class InstanceMap
{
    private ImHashMap<InstanceIdentifier, object> _hashMap;
    private ConcurrentDictionary<InstanceIdentifier, object> _dictionary;
    
    public InstanceMap(InstanceMapBehavior behavior)
    {
        if (behavior == InstanceMapBehavior.Default)
        {
            _dictionary = null;
            _hashMap = ImHashMap<InstanceIdentifier, object>.Empty;
        }
        else
        {
            _hashMap = null;
            _dictionary = new ConcurrentDictionary<InstanceIdentifier, object>();
        }
    }
    
    public bool TryFind(InstanceIdentifier key, out object value) => _hashMap != null
        ? _hashMap.TryFind(key, out value)
        : _dictionary.TryGetValue(key, out value);

    public InstanceMap AddOrUpdate(InstanceIdentifier hash, object value)
    {
        if (_hashMap != null)
        {
            _hashMap = _hashMap.AddOrUpdate(hash, value);
        }
        else
        {
            _dictionary = new ConcurrentDictionary<InstanceIdentifier, object>(_dictionary)
            {
                [hash] = value
            };
        }

        return this;
    }
}