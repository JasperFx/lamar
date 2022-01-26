using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration
{
    public static class ReflectionExtensions
    {
        public static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>
        {
            {typeof(int), "int"},
            {typeof(void), "void"},
            {typeof(string), "string"},
            {typeof(long), "long"},
            {typeof(double), "double"},
            {typeof(bool), "bool"},
            {typeof(object), "object"},
            {typeof(object[]), "object[]"}
        };
        
        public static bool IsAsync(this MethodInfo method)
        {
            if (method.ReturnType == null)
            {
                return false;
            }

            return method.ReturnType == typeof(Task) || method.ReturnType.Closes(typeof(Task<>));

        }

        public static bool CanBeOverridden(this MethodInfo method)
        {
            if (method.IsAbstract) return true;

            if (method.IsVirtual && !method.IsFinal) return true;

            return false;
        }
        
        /// <summary>
        /// Derives the full type name *as it would appear in C# code*
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FullNameInCode(this Type type)
        {
            if (Aliases.ContainsKey(type)) return Aliases[type];
            
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First();
                if(type.IsNested && type.DeclaringType?.IsGenericTypeDefinition == true)
                {
                    cleanName = $"{type.ReflectedType.NameInCode(type.GetGenericArguments())}.{cleanName}";
                    return $"{type.Namespace}.{cleanName}";
                }

                if (type.IsNested)
                {
                    cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                }

                var args = type.GetGenericArguments().Select(x => x.FullNameInCode()).Join(", ");

                return $"{type.Namespace}.{cleanName}<{args}>";
            }

            if (type.FullName == null)
            {
                return type.Name;
            }
            
            return type.FullName.Replace("+", ".");
        }

        /// <summary>
        /// Derives the type name *as it would appear in C# code*
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string NameInCode(this Type type)
        {
            if (Aliases.ContainsKey(type)) return Aliases[type];
            
            if (type.IsGenericType)
            {
                if (type.IsGenericTypeDefinition)
                {
                    var parts = type.Name.Split('`');;
                    var cleanName = parts.First().Replace("+", ".");

                    var hasArgs = parts.Length > 1;
                    if (hasArgs) 
                    {
                        var numberOfArgs = int.Parse(parts[1]) - 1;
                        cleanName = $"{cleanName}<{"".PadLeft(numberOfArgs, ',')}>";
                    }

                    if (type.IsNested)
                    {
                        cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                    }

                    return cleanName;
                }
                else
                {
                    var cleanName = type.Name.Split('`').First().Replace("+", ".");
                    if (type.IsNested)
                    {
                        cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                    }
                    
                    var args = type.GetGenericArguments().Select(x => x.FullNameInCode()).Join(", ");

                    return $"{cleanName}<{args}>";
                }
            }

            if (type.MemberType == MemberTypes.NestedType)
            {
                return $"{type.ReflectedType.NameInCode()}.{type.Name}";
            }

            return type.Name.Replace("+", ".").Replace("`", "_");
        }

        /// <summary>
        /// Derives the type name *as it would appear in C# code* for a type with generic parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericParameterTypes"></param>
        /// <returns></returns>
        public static string NameInCode(this Type type, Type[] genericParameterTypes)
        {
            var cleanName = type.Name.Split('`').First().Replace("+", ".");
            var args = genericParameterTypes.Select(x => x.FullNameInCode()).Join(", ");
            return $"{cleanName}<{args}>";
        }
        
        public static string ShortNameInCode(this Type type)
        {
            if (Aliases.ContainsKey(type)) return Aliases[type];

            try
            {
                if (type.IsGenericType)
                {
                    if (type.IsGenericTypeDefinition)
                    {
                        var parts = type.Name.Split('`');
                
                        var cleanName = parts.First().Replace("+", ".");

                        var hasArgs = parts.Length > 1;
                        if (hasArgs) 
                        {
                            var numberOfArgs = int.Parse(parts[1]) - 1;
                            cleanName = $"{cleanName}<{"".PadLeft(numberOfArgs, ',')}>";
                        }
                        if (type.IsNested)
                        {
                            cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                        }

                        return cleanName;
                    }
                    else
                    {
                        var cleanName = type.Name.Split('`').First().Replace("+", ".");
                        if (type.IsNested)
                        {
                            cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                        }

                        var args = type.GetGenericArguments().Select(x => x.ShortNameInCode()).Join(", ");

                        return $"{cleanName}<{args}>";
                    }
                }

                if (type.MemberType == MemberTypes.NestedType)
                {
                    return $"{type.ReflectedType.NameInCode()}.{type.Name}";
                }

                return type.Name.Replace("+", ".");
            }
            catch (Exception)
            {
                return type.Name;
            }
        }

        /// <summary>
        /// Creates a deterministic class name for the supplied type
        /// and suffix. Uses a hash of the type's full name to disambiguate
        /// between derivations on the same original type name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string ToSuffixedTypeName(this Type type, string suffix)
        {
            var prefix = type.Name.Split('`').First();
            var hash = Math.Abs(type.FullNameInCode().GetStableHashCode());
            return $"{prefix}{suffix}{hash}";
        }
        
        public static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for(int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i+1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
                }

                return hash1 + (hash2*1566083941);
            }
        }
    }
}
