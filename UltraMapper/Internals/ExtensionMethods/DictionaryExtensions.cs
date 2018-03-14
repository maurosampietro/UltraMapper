using System;
using System.Collections.Generic;

namespace UltraMapper.Internals
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory )
        {
            TValue value;

            if( !dictionary.TryGetValue( key, out value ) )
            {
                value = valueFactory();
                dictionary.Add( key, value );
            }

            return value;
        }
    }
}
