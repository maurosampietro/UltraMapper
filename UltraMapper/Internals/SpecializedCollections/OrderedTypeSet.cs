using System;
using System.Collections;
using System.Collections.Generic;

namespace UltraMapper.Internals
{
    /// <summary>
    /// Represents a collection  where each element is unique by type 
    /// and insertion order is preserved.
    /// </summary>
    public sealed class OrderedTypeSet<T> : ICollection<T>
    {
        private readonly Dictionary<Type, LinkedListNode<T>> _dictionary;
        private readonly LinkedList<T> _linkedList;

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public OrderedTypeSet()
        {
            _dictionary = new Dictionary<Type, LinkedListNode<T>>();
            _linkedList = new LinkedList<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains( T item )
        {
            return _dictionary.ContainsKey( item.GetType() );
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            _linkedList.CopyTo( array, arrayIndex );
        }

        public bool Add( T item )
        {
            if( _dictionary.ContainsKey( item.GetType() ) )
                return false;

            var node = _linkedList.AddLast( item );
            _dictionary.Add( item.GetType(), node );

            return true;
        }

        public void AddBefore<TMappingExpressionBuilder>(
           params T[] mebs )
        {
            var node = _dictionary[ typeof( TMappingExpressionBuilder ) ];

            for( int i = 0; i < mebs.Length; i++ )
            {
                var newNode = _linkedList.AddBefore( node, mebs[ i ] );
                _dictionary.Add( mebs[ i ].GetType(), newNode );
            }
        }

        public void AddAfter<TMappingExpressionBuilder>(
            params T[] mebs )
        {
            var node = _dictionary[ typeof( TMappingExpressionBuilder ) ];

            for( int i = 0; i < mebs.Length; i++ )
            {
                var nextNode = new LinkedListNode<T>( mebs[ i ] );
                _linkedList.AddBefore( node, mebs[i]);
                _dictionary.Add( mebs[ i ].GetType(), nextNode );
                node = nextNode;
            }
        }

        public bool Remove( T item )
        {
            bool found = _dictionary.TryGetValue( item.GetType(),
                out LinkedListNode<T> node );

            if( !found ) return false;

            _dictionary.Remove( item.GetType() );
            _linkedList.Remove( node );

            return true;
        }

        public void Clear()
        {
            _dictionary.Clear();
            _linkedList.Clear();
        }

        void ICollection<T>.Add( T item )
        {
            Add( item );
        }
    }
}
