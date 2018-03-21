using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lamar.Util;

namespace Lamar.Codegen
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
            {typeof(Task), "Task"},
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
        

        public static string FullNameInCode(this Type type)
        {
            if (Aliases.ContainsKey(type)) return Aliases[type];
            
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First();
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

        public static string NameInCode(this Type type)
        {
            if (Aliases.ContainsKey(type)) return Aliases[type];
            
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First().Replace("+", ".");
                if (type.IsNested)
                {
                    cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                }
                
                var args = type.GetGenericArguments().Select(x => x.FullNameInCode()).Join(", ");

                return $"{cleanName}<{args}>";
            }

            if (type.MemberType == MemberTypes.NestedType)
            {
                return $"{type.ReflectedType.NameInCode()}.{type.Name}";
            }

            return type.Name.Replace("+", ".");
        }
        
        public static string ShortNameInCode(this Type type)
        {
            if (Aliases.ContainsKey(type)) return Aliases[type];
            
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var cleanName = type.Name.Split('`').First().Replace("+", ".");
                if (type.IsNested)
                {
                    cleanName = $"{type.ReflectedType.NameInCode()}.{cleanName}";
                }
                
                var args = type.GetGenericArguments().Select(x => x.ShortNameInCode()).Join(", ");

                return $"{cleanName}<{args}>";
            }

            if (type.MemberType == MemberTypes.NestedType)
            {
                return $"{type.ReflectedType.NameInCode()}.{type.Name}";
            }

            return type.Name.Replace("+", ".");
        }


    }
}
