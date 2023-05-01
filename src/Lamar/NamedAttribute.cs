using System;

namespace Lamar;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter)]
public class NamedAttribute : Attribute
{
    public NamedAttribute(string name)
    {
        Name = name;
    }


    public NamedAttribute(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    public string Name { get; }
    public string TypeName { get; set; }
}