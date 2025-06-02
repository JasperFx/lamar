using System;

namespace Lamar.IoC;

public readonly struct InstanceIdentifier
{
    public string Name { get; } = null;
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
        
        if (Name == null)
        {
            return ServiceType.GetHashCode();
        }

        unchecked
        {
            return (ServiceType.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }

    private bool Equals(InstanceIdentifier other)
    {
        if (Name == null && other.Name == null)
        {
            return ServiceType == other.ServiceType;
        }
        else if (Name != null && other.Name != null)
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