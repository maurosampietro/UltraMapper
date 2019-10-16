using System;
using System.Collections.Generic;

namespace UltraMapper.Internals
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory )
        {
            if( !dictionary.TryGetValue( key, out TValue value ) )
            {
                value = valueFactory();
                dictionary.Add( key, value );
            }

            return value;
        }
    }
}
