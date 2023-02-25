using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        public static TValue OverWrite<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory )
        {
            var value = valueFactory();
            dictionary[ key ] = value;
            return value;
        }
    }
}