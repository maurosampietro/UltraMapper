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
        internal static void Update<TSourceElement, TTargetElement>( Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, ICollection<TTargetElement> target, Func<TSourceElement, TTargetElement, bool> comparer )
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
                        mapper.Map( sourceItem, targetItem, referenceTracker );
                        addToList = false; //we already updated
                        break; //next source item
                    }
                }

                if( addToList )
                {
                    var targetItem = new TTargetElement();
                    mapper.Map( sourceItem, targetItem, referenceTracker );
                    target.Add( targetItem );
                }
            }
        }
    }
}
