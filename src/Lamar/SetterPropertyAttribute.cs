using System;

namespace Lamar;

/// <summary>
///     Marks a Property in a concrete class as filled by setter injection
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SetterPropertyAttribute : Attribute
{
}