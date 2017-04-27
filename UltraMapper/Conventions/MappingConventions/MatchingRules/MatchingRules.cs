using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MatchingRules : IEnumerable<IMatchingRule>
    {
        protected Dictionary<Type, IMatchingRule> _propertyMatchingRules;

        public MatchingRules( Action<MatchingRules> config )
                : this() { config?.Invoke( this ); }

        public MatchingRules()
        {
            _propertyMatchingRules = new Dictionary<Type, IMatchingRule>();
        }

        public MatchingRules GetOrAdd<T>(
            Action<T> ruleConfig = null ) where T : IMatchingRule, new()
        {
            _propertyMatchingRules.GetOrAdd( typeof( T ), () =>
            {
                T instance = new T();
                ruleConfig?.Invoke( instance );

                return instance;
            } );

            return this;
        }

        public MatchingRules Remove<T>() where T : IMatchingRule
        {
            _propertyMatchingRules.Remove( typeof( T ) );
            return this;
        }

        public IEnumerator<IMatchingRule> GetEnumerator()
        {
            return _propertyMatchingRules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
