using System;
using System.Linq;
using System.Linq.Expressions;
using JasperFx.Core.Reflection;

namespace Lamar.Util;

internal static class TypeExtensions
{
    private readonly static Type[] _ignoredTypes = new[] { typeof(Uri), typeof(TimeSpan), typeof(DateTimeOffset) };
    
    internal static bool ShouldIgnore(this Type type)
    {
        if (type.IsSimple()) return true;

        if (type.IsEnum) return true;

        if (_ignoredTypes.Contains(type)) return true;
        if (type.IsNullable() && _ignoredTypes.Contains(type.GetGenericArguments().First())) return true;

        if (type.CanBeCastTo<Expression>()) return true;

        if (type.IsDateTime()) return true;

        return false;
    }
}