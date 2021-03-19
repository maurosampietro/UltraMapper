using System;
using System.Collections;
using System.Collections.Generic;

namespace UltraMapper.Internals
{
    public class SingletonList<TInterface> : IEnumerable<TInterface>
    {
        protected Dictionary<Type, TInterface> _instances;

        public SingletonList( Action<SingletonList<TInterface>> config = null )
        {
            _instances = new Dictionary<Type, TInterface>();
            config?.Invoke( this );
        }

        public SingletonList<TInterface> GetOrAdd<T>( Action<T> config = null )
            where T : TInterface, new()
        {
            var instance = _instances.GetOrAdd( typeof( T ), () => new T() );
            config?.Invoke( (T)instance );

            return this;
        }

        public SingletonList<TInterface> Remove<T>() where T : TInterface
        {
            _instances.Remove( typeof( T ) );
            return this;
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
