using System;

namespace Lamar;

/// <summary>
///     Use to direct Lamar type scanning to ignore this type
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class LamarIgnoreAttribute : Attribute
{
}