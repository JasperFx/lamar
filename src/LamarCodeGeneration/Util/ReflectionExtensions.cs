using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LamarCodeGeneration.Util
{
    public static class ReflectionExtensions
    {
        
        public static bool IsEnumerable(this Type type)
        {
            if (type.IsArray) return true;

            return type.IsGenericType && _enumerableTypes.Contains(type.GetGenericTypeDefinition());
        }

        public static Type DetermineElementType(this Type serviceType)
        {
            if (serviceType.IsArray)
            {
                return serviceType.GetElementType();
            }

            return serviceType.GetGenericArguments().First();
        }
        
        private static readonly List<Type> _enumerableTypes = new List<Type>
        {
            typeof (IEnumerable<>),
            typeof (IList<>),
            typeof (IReadOnlyList<>),
            typeof (List<>)
        };

        public static T GetAttribute<T>(this MemberInfo provider) where T : Attribute
        {
            var atts = provider.GetCustomAttributes(typeof (T), true);
            return atts.FirstOrDefault() as T;
        }

        public static T GetAttribute<T>(this Assembly provider) where T : Attribute
        {
            var atts = provider.GetCustomAttributes(typeof(T));
            return atts.FirstOrDefault() as T;
        }

        public static T GetAttribute<T>(this Module provider) where T : Attribute
        {
            var atts = provider.GetCustomAttributes(typeof(T));
            return atts.FirstOrDefault() as T;
        }

        public static T GetAttribute<T>(this ParameterInfo provider) where T : Attribute
        {
            var atts = provider.GetCustomAttributes(typeof(T), true);
            return atts.FirstOrDefault() as T;
        }

        public static IEnumerable<T> GetAllAttributes<T>(this Assembly provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T)).OfType<T>();
        }

        public static IEnumerable<T> GetAllAttributes<T>(this MemberInfo provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), true).OfType<T>();
        }

        public static IEnumerable<T> GetAllAttributes<T>(this Module provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T)).OfType<T>();
        }

        public static IEnumerable<T> GetAllAttributes<T>(this ParameterInfo provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), true).OfType<T>();
        }

        public static bool HasAttribute<T>(this Assembly provider) where T : Attribute
        {
            return provider.IsDefined(typeof(T));
        }

        public static bool HasAttribute<T>(this MemberInfo provider) where T : Attribute
        {
            return provider.IsDefined(typeof(T), true);
        }

        public static bool HasAttribute<T>(this Module provider) where T : Attribute
        {
            return provider.IsDefined(typeof(T));
        }

        public static bool HasAttribute<T>(this ParameterInfo provider) where T : Attribute
        {
            return provider.IsDefined(typeof(T), true);
        }

        public static void ForAttribute<T>(this Assembly provider, Action<T> action) where T : Attribute
        {
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
            }
        }

        public static void ForAttribute<T>(this MemberInfo provider, Action<T> action) where T : Attribute
        {
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
            }
        }

        public static void ForAttribute<T>(this Module provider, Action<T> action) where T : Attribute
        {
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
            }
        }

        public static void ForAttribute<T>(this ParameterInfo provider, Action<T> action) where T : Attribute
        {
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
            }
        }

        public static void ForAttribute<T>(this Assembly provider, Action<T> action, Action elseDo)
            where T : Attribute
        {
            var found = false;
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
                found = true;
            }

            if (!found) elseDo();
        }

        public static void ForAttribute<T>(this MemberInfo provider, Action<T> action, Action elseDo)
            where T : Attribute
        {
            var found = false;
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
                found = true;
            }

            if (!found) elseDo();
        }

        public static void ForAttribute<T>(this Module provider, Action<T> action, Action elseDo)
            where T : Attribute
        {
            var found = false;
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
                found = true;
            }

            if (!found) elseDo();
        }

        public static void ForAttribute<T>(this ParameterInfo provider, Action<T> action, Action elseDo)
            where T : Attribute
        {
            var found = false;
            foreach (T attribute in provider.GetAllAttributes<T>())
            {
                action(attribute);
                found = true;
            }

            if (!found) elseDo();
        }


    }
}