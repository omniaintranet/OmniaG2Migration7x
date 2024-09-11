using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static void AddIfNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) where TValue: class
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return;

            dict.Add(key, value);
        }
    }
}
