using System;
using System.Collections;
using System.Collections.Generic;

namespace UltraMapper.Internals
{
    /// <summary>
    /// Represents a collection where each element is unique by type.
    /// </summary>
    public class TypeSet<TInterface> : IEnumerable<TInterface>
    {
        protected Dictionary<Type, TInterface> _instances;

        public TypeSet( Action<TypeSet<TInterface>> config = null )
        {
            _instances = new Dictionary<Type, TInterface>();
            config?.Invoke( this );
        }

        public TypeSet<TInterface> GetOrAdd<T>( Action<T> config = null )
            where T : TInterface, new()
        {
            var instance = _instances.GetOrAdd( typeof( T ), () => new T() );
            config?.Invoke( (T)instance );

            return this;
        }

        public TypeSet<TInterface> Remove<T>() where T : TInterface
        {
            _instances.Remove( typeof( T ) );
            return this;
        }

        public void Clear()
        {
            _instances.Clear();
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return _instances.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
