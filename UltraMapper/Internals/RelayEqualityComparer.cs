using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals
{
    internal class RelayEqualityComparer<T> : IEqualityComparer<T>
    {
        public readonly Func<T, T, bool> Comparer;

        public RelayEqualityComparer( Func<T, T, bool> comparer )
        {
            this.Comparer = comparer;
        }

        public bool Equals( T x, T y ) => this.Comparer( x, y );
        public int GetHashCode( T obj ) => -1; //always force the execution of Equals
    }
}
