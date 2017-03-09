using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.ExtensionMethods
{
    public static class DictionaryExtensions
    {
        public static void UpdateOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if( dictionary.ContainsKey( key ) )
                dictionary[ key ] = value;
            else
                dictionary.Add( key, value );
        }

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
