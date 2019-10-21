using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UltraMapper.Internals.ExtensionMethods
{
    internal static class LinqExtensions
    {
        internal static void Update<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, ICollection<TTargetElement> target,
            Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
            if( target is TTargetElement[] array )
            {
                ArrayUpdate( mapper, referenceTracker, source, array, comparer );
            }
            else
            {
                CollectionUpdate( mapper, referenceTracker, source, target, comparer );
            }
        }

        private static void CollectionUpdate<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, ICollection<TTargetElement> target,
            Func<TSourceElement, TTargetElement, bool> comparer )
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
            var itemsToAdd = new List<TTargetElement>();
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

        private static void ArrayUpdate<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, TTargetElement[] target,
            Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
            //Search for items to remove and set the array element to null.
            //Save the index of the removed element.

            var removedItemIndexes = new List<int>();

            for( int i = 0; i < target.Length; i++ )
            {
                var targetItem = target[ i ];
                bool remove = true;
                foreach( var sourceItem in source )
                {
                    if( sourceItem == null )
                        continue;

                    if( comparer( sourceItem, targetItem ) )
                        remove = false;
                }

                if( remove )
                {
                    removedItemIndexes.Add( i );
                    target[ i ] = null;
                }
            }

            //search items to add, map and add them
            var itemsToAdd = new List<TTargetElement>();
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
            //Elements are inserted in the first empty spot in the array
            foreach( var item in itemsToAdd )
            {
                bool elementInserted = false;
                for( int i = 0; i < target.Length; i++ )
                {
                    if( target[ i ] == null )
                    {
                        target[ i ] = item;
                        elementInserted = true;
                        break;
                    }
                }

                if( !elementInserted )
                    throw new Exception( "Could not find an empty spot. Array size is not the correct." );
            }
        }
    }
}
