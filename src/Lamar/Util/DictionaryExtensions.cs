using System;
using System.Collections.Generic;

namespace Lamar.Util
{
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// This is a big THANK YOU to the BCL for not hooking a brotha' up
        /// This add will tell WHAT KEY you added twice.
        /// </summary>
        public static void SmartAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            try
            {
                dictionary.Add(key, value);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("The key '{0}' already exists.".ToFormat(key), e);
            }
        }
    }
}