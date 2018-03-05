using System;

namespace Lamar
{
    /// <summary>
    /// Marks a Property in a Pluggable class as filled by setter injection 
    /// </summary>
    [Obsolete("Not sure yet if we'll support this later")]
    [AttributeUsage(AttributeTargets.Property)]
    public class SetterPropertyAttribute : Attribute
    {
    }
}