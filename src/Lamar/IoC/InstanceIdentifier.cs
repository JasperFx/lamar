using System;

namespace Lamar.IoC;

public readonly struct InstanceIdentifier : IEquatable<InstanceIdentifier>
{
    public string Name { get; }
    public Type ServiceType { get; }

    public InstanceIdentifier(string name, Type serviceType)
    {
        Name = name;
        ServiceType = serviceType;
    }

    public override bool Equals(object obj)
    {
        return obj is InstanceIdentifier identifier && Equals(identifier);
    }

    public override int GetHashCode()
    {
        if (ServiceType == null)
        {
            return default;
        }
        
        return (ServiceType.GetHashCode() * 397) ^ (Name ?? "default").GetHashCode();
    }

    public bool Equals(InstanceIdentifier other)
    {
        if (Name == null && other.Name == null)
        {
            return ServiceType == other.ServiceType;
        }

        if (Name != null && other.Name != null)
        {
            return ServiceType == other.ServiceType && Name.Equals(other.Name);
        }

        return false;
    }

    public static bool operator ==(InstanceIdentifier left, InstanceIdentifier right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(InstanceIdentifier left, InstanceIdentifier right)
    {
        return !(left == right);
    }
}