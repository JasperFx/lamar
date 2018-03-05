using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lamar.Util
{
    /// <summary>
    /// Taken directly from Marten: https://github.com/JasperFx/marten/blob/2f18d09fa2034cbc647f48a74bbf3bbb8ea51116/src/Marten/Util/EnumerableExtensions.cs
    /// </summary>
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle = true)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
            {
                Visit(item, visited, sorted, dependencies, throwOnCycle);
            }

            return sorted;
        }

        private static void Visit<T>(T item, ISet<T> visited, ICollection<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
        {
            if (visited.Contains(item))
            {
                if (throwOnCycle && !sorted.Contains(item))
                {
                    throw new Exception("Cyclic dependency found");
                }
            }
            else
            {
                visited.Add(item);

                foreach (var dep in dependencies(item))
                {
                    Visit(dep, visited, sorted, dependencies, throwOnCycle);
                }

                sorted.Add(item);
            }
        }
        
        
        
        /// <summary>
        /// Adds the value to the list if it does not already exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="value"></param>
        public static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value)) return;
            list.Add(value);
        }

        /// <summary>
        /// Adds a series of values to a list if they do not already exist in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="values"></param>
        public static void Fill<T>(this IList<T> list, IEnumerable<T> values)
        {
            list.AddRange(values.Where(v => !list.Contains(v)));
        }

        /// <summary>
        /// Removes all of the items that match the provided condition
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to modify</param>
        /// <param name="whereEvaluator">The test to determine if an item should be removed</param>
        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> whereEvaluator)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (whereEvaluator(list[i])) list.RemoveAt(i);
            }
        }

        /// <summary>
        /// Concatenates a string between each item in a list of strings
        /// </summary>
        /// <param name="values">The array of strings to join</param>
        /// <param name="separator">The value to concatenate between items</param>
        /// <returns></returns>
        public static string Join(this string[] values, string separator)
        {
            return string.Join(separator, values);
        }

        /// <summary>
        /// Concatenates a string between each item in a sequence of strings
        /// </summary>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join(this IEnumerable<string> values, string separator)
        {
            return Join(values.ToArray(), separator);
        }

        /// <summary>
        /// Performs an action with a counter for each item in a sequence and provides
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence</typeparam>
        /// <param name="values">The sequence to iterate</param>
        /// <param name="eachAction">The action to performa on each item</param>
        /// <returns></returns>
        public static IEnumerable<T> Each<T>(this IEnumerable<T> values, Action<T, int> eachAction)
        {
            int index = 0;
            foreach (T item in values)
            {
                eachAction(item, index++);
            }

            return values;
        }

        [DebuggerStepThrough]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> values, Action<T> eachAction)
        {
            foreach (T item in values)
            {
                eachAction(item);
            }

            return values;
        }

        [DebuggerStepThrough]
        public static IEnumerable Each(this IEnumerable values, Action<object> eachAction)
        {
            foreach (object item in values)
            {
                eachAction(item);
            }

            return values;
        }

        /// <summary>
        /// Returns the first non-null value from executing the func against the enumerable
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TReturn FirstValue<TItem, TReturn>(this IEnumerable<TItem> enumerable, Func<TItem, TReturn> func)
            where TReturn : class
        {
            foreach (TItem item in enumerable)
            {
                TReturn @object = func(item);
                if (@object != null) return @object;
            }

            return null;
        }

        /// <summary>
        /// Add many items to a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IList<T> AddMany<T>(this IList<T> list, params T[] items)
        {
            return list.AddRange(items);
        }

        /// <summary>
        /// Appends a sequence of items to an existing list
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to modify</param>
        /// <param name="items">The sequence of items to add to the list</param>
        /// <returns></returns>
        public static IList<T> AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            items.Each(list.Add);
            return list;
        }

        public static IEnumerable<T> UnionWith<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.Union(second);
        } 
        
        
    }
}
