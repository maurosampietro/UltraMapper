using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

            var itemsToAdd = new List<TTargetElement>();

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
                    itemsToAdd.Add( targetItem );
                }
            }

            //it is important to add the items after the target collection
            //has been updated (or it can happen that we update items that only needed 
            //to be added acting more or less like a hashset).
            foreach( var item in itemsToAdd )
                target.Add( item );
        }
    }
}
