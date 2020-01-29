using System;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Internals.ExtensionMethods
{
    public static class LinqExtensions
    {
        public static void Update<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, ICollection<TTargetElement> target,
            Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
            switch( target )
            {
                case TTargetElement[] array:
                    ArrayUpdate( mapper, referenceTracker, source, array, comparer );
                    break;

                case Queue<TTargetElement> queue:
                    UpdateQueue( mapper, referenceTracker, source, queue, comparer );
                    break;

                case Stack<TTargetElement> stack:
                    UpdateStack( mapper, referenceTracker, source, stack, comparer );
                    break;

                default:
                    CollectionUpdate( mapper, referenceTracker, source, target, comparer );
                    break;
            }
        }

        private static void MapItems<TSourceElement, TTargetElement>( Mapper mapper, 
            ReferenceTracking referenceTracker, IEnumerable<TSourceElement> source, 
            IEnumerable<TTargetElement> target, Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
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
                }
            }
        }

        public static void CollectionUpdate<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, ICollection<TTargetElement> target,
            Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
            MapItems( mapper, referenceTracker, source, target, comparer );

            target.Clear();
            foreach( var item in source )
            {
                var targetItem = (TTargetElement)referenceTracker[ item, typeof( TTargetElement ) ];
                target.Add( targetItem );
            }
        }

        public static void ArrayUpdate<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, TTargetElement[] target,
            Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
            MapItems( mapper, referenceTracker, source, target, comparer );

            var arrayTarget = target as TTargetElement[];
            var sourceCount = source.Count();
            if( sourceCount > arrayTarget.Length )
                arrayTarget = target = new TTargetElement[ sourceCount ];

            int index = 0;
            foreach( var item in source )
            {
                var targetItem = (TTargetElement)referenceTracker[ item, typeof( TTargetElement ) ];
                target[ index++ ] = targetItem;
            }
        }

        public static void UpdateQueue<TSourceElement, TTargetElement>(
            Mapper mapper, ReferenceTracking referenceTracker,
            IEnumerable<TSourceElement> source, Queue<TTargetElement> target,
            Func<TSourceElement, TTargetElement, bool> comparer )
            where TSourceElement : class
            where TTargetElement : class, new()
        {
            MapItems( mapper, referenceTracker, source, target, comparer );

            target.Clear();
            foreach( var item in source )
            {
                var targetItem = (TTargetElement)referenceTracker[ item, typeof( TTargetElement ) ];
                target.Enqueue( targetItem );
            }
        }

        public static void UpdateStack<TSourceElement, TTargetElement>(
           Mapper mapper, ReferenceTracking referenceTracker,
           IEnumerable<TSourceElement> source, Stack<TTargetElement> target,
           Func<TSourceElement, TTargetElement, bool> comparer )
           where TSourceElement : class
           where TTargetElement : class, new()
        {
            MapItems( mapper, referenceTracker, source, target, comparer );

            var tempStack = new Stack<TTargetElement>();
            foreach( var item in source )
            {
                var targetItem = (TTargetElement)referenceTracker[ item, typeof( TTargetElement ) ];
                tempStack.Push( targetItem );
            }

            target.Clear();

            foreach( var item in tempStack )
                target.Push( item );
        }
    }
}
