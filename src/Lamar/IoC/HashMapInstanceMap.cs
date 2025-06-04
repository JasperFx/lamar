using ImTools;

namespace Lamar.IoC;

public class HashMapInstanceMap : InstanceMap
{
    private ImHashMap<InstanceIdentifier, object> _hashMap = ImHashMap<InstanceIdentifier, object>.Empty;

    public override bool TryFind(InstanceIdentifier key, out object value) => _hashMap.TryFind(key, out value);

    public override HashMapInstanceMap AddOrUpdate(InstanceIdentifier hash, object value)
    {
        _hashMap = _hashMap.AddOrUpdate(hash, value);

        return this;
    }
}