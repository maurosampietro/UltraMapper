using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals.ExtensionMethods
{
    internal static class LinqExtensions
    {
        public static void Update<TSourceElement, TTargetElement>( UltraMapper mapper, IEnumerable<TSourceElement> source,
            ICollection<TTargetElement> target, Func<TSourceElement, TTargetElement, bool> comparer ) 
            where TSourceElement : class 
            where TTargetElement : class, new()
        {
            //search items to remove...
            var itemsToRemove = new List<TTargetElement>();
            foreach( var targetItem in target )
            {
                bool remove = true;
                foreach( var sourceItem in source )
                {
                    if( comparer( sourceItem, targetItem ) )
                        remove = false;
                }

                if( remove )
                    itemsToRemove.Add( targetItem );
            }

            //..and remove them
            foreach( var item in itemsToRemove )
                target.Remove( item );

            //search items to add, map and add them
            foreach( var sourceItem in source )
            {
                bool addToList = true;
                foreach( var targetItem in target )
                {
                    if( comparer( sourceItem, targetItem ) )
                    {
                        mapper.Map( sourceItem, targetItem );
                        addToList = false; //we already updated
                        break; //next source item
                    }
                }

                if( addToList )
                    target.Add( mapper.Map<TTargetElement>( sourceItem ) );
            }
        }
    }
}
