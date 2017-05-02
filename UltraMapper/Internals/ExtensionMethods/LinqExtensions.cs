using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals.ExtensionMethods
{
    public static class LinqExtensions
    {
        public static void Update<T>( UltraMapper mapper, IEnumerable<T> source,
            ICollection<T> target, IEqualityComparer<T> comparer ) where T : class
        {
            var itemsToRemove = target.Except( source, comparer ).ToList();
            foreach( var item in itemsToRemove ) target.Remove( item );

            List<T> itemsToAdd = new List<T>();
            foreach( var sourceItem in source )
            {
                bool addToList = true;
                foreach( var targetItem in target )
                {
                    if( comparer.Equals( sourceItem, targetItem ) )
                    {
                        mapper.Map( sourceItem, targetItem );
                        addToList = false; //we already updated
                        break; //next source item
                    }
                }

                if( addToList )
                    itemsToAdd.Add( sourceItem );
            }

            foreach( var item in itemsToAdd ) target.Add( item );
        }
    }
}
