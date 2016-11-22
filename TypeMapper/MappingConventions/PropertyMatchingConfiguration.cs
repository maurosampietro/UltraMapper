using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.MappingConventions.PropertyMatchingRules;

namespace TypeMapper.MappingConventions
{
    public class PropertyMatchingConfiguration : IEnumerable<IPropertyMatchingRule>
    {
        public Dictionary<Type, IPropertyMatchingRule> _propertyMatchingRules { get; protected set; }
        public Func<PropertyInfo, PropertyInfo, bool> MatchingEvaluator { get; protected set; }

        protected PropertyMatchingConfiguration( Dictionary<Type, IPropertyMatchingRule> propertyMatchingRules )
        {
            _propertyMatchingRules = propertyMatchingRules;
            this.RespectAll();
        }

        public PropertyMatchingConfiguration()
            : this( new Dictionary<Type, IPropertyMatchingRule>() ) { }

        public PropertyMatchingConfiguration<T> GetOrAdd<T>( Action<T> ruleConfig = null ) where T : IPropertyMatchingRule, new()
        {
            var type = typeof( T );

            IPropertyMatchingRule instance;
            if( !_propertyMatchingRules.TryGetValue( type, out instance ) )
                _propertyMatchingRules.Add( type, instance = new T() );

            ruleConfig?.Invoke( (T)instance );
            return new PropertyMatchingConfiguration<T>( _propertyMatchingRules );
        }

        public PropertyMatchingConfiguration Remove<T>() where T : IPropertyMatchingRule
        {
            _propertyMatchingRules.Remove( typeof( T ) );
            return this;
        }

        public void RespectAll()
        {
            this.MatchingEvaluator = new Func<PropertyInfo, PropertyInfo, bool>( ( source, target ) =>
               _propertyMatchingRules.Values.All( rule => rule.IsCompliant( source, target ) ) );
        }

        public void RespectAny()
        {
            this.MatchingEvaluator = new Func<PropertyInfo, PropertyInfo, bool>( ( source, target ) =>
               _propertyMatchingRules.Values.Any( rule => rule.IsCompliant( source, target ) ) );
        }

        public IEnumerator<IPropertyMatchingRule> GetEnumerator()
        {
            return _propertyMatchingRules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class PropertyMatchingConfiguration<T1> : PropertyMatchingConfiguration
    {
        public PropertyMatchingConfiguration( Dictionary<Type, IPropertyMatchingRule> propertyMatchingRules )
            : base( propertyMatchingRules ) { }

        new public PropertyMatchingConfiguration<T1, T2> GetOrAdd<T2>(
            Action<T2> ruleConfig = null ) where T2 : IPropertyMatchingRule, new()
        {
            base.GetOrAdd( ruleConfig );
            return new PropertyMatchingConfiguration<T1, T2>( _propertyMatchingRules );
        }
    }

    public class PropertyMatchingConfiguration<T1, T2> : PropertyMatchingConfiguration
    {
        public PropertyMatchingConfiguration( Dictionary<Type, IPropertyMatchingRule> propertyMatchingRules )
            : base( propertyMatchingRules ) { }

        new public PropertyMatchingConfiguration<T1, T2, T3> GetOrAdd<T3>(
            Action<T3> ruleConfig = null ) where T3 : IPropertyMatchingRule, new()
        {
            base.GetOrAdd( ruleConfig );
            return new PropertyMatchingConfiguration<T1, T2, T3>( _propertyMatchingRules );
        }

        public void Respect( Func<T1, T2, RuleChaining> matchCondition )
        {
            var t1Instance = (T1)_propertyMatchingRules[ typeof( T1 ) ];
            var t2instance = (T2)_propertyMatchingRules[ typeof( T2 ) ];

            this.MatchingEvaluator = matchCondition( t1Instance, 
                t2instance ).Expression.Compile();
        }
    }

    public class PropertyMatchingConfiguration<T1, T2, T3> : PropertyMatchingConfiguration
    {
        public PropertyMatchingConfiguration( Dictionary<Type, IPropertyMatchingRule> propertyMatchingRules )
            : base( propertyMatchingRules ) { }

        new public PropertyMatchingConfiguration<T1, T2, T3, T4> GetOrAdd<T4>(
            Action<T4> ruleConfig = null ) where T4 : IPropertyMatchingRule, new()
        {
            base.GetOrAdd( ruleConfig );
            return new PropertyMatchingConfiguration<T1, T2, T3, T4>( _propertyMatchingRules );
        }

        public void Respect( Func<T1, T2, T3, RuleChaining> matchCondition )
        {
            var t1Instance = (T1)_propertyMatchingRules[ typeof( T1 ) ];
            var t2instance = (T2)_propertyMatchingRules[ typeof( T2 ) ];
            var t3instance = (T3)_propertyMatchingRules[ typeof( T3 ) ];

            this.MatchingEvaluator = matchCondition( t1Instance, 
                t2instance, t3instance ).Expression.Compile();
        }
    }

    public class PropertyMatchingConfiguration<T1, T2, T3, T4> : PropertyMatchingConfiguration
    {
        public PropertyMatchingConfiguration( Dictionary<Type, IPropertyMatchingRule> propertyMatchingRules )
            : base( propertyMatchingRules ) { }

        new public PropertyMatchingConfiguration<T1, T2, T3, T4> GetOrAdd<T5>(
            Action<T5> ruleConfig = null ) where T5 : IPropertyMatchingRule, new()
        {
            base.GetOrAdd( ruleConfig );
            return this;
        }

        public void Respect( Func<T1, T2, T3, T4, RuleChaining> matchCondition )
        {
            var t1Instance = (T1)_propertyMatchingRules[ typeof( T1 ) ];
            var t2instance = (T2)_propertyMatchingRules[ typeof( T2 ) ];
            var t3instance = (T3)_propertyMatchingRules[ typeof( T3 ) ];
            var t4Instance = (T4)_propertyMatchingRules[ typeof( T4 ) ];

            this.MatchingEvaluator = matchCondition( t1Instance, 
                t2instance, t3instance, t4Instance ).Expression.Compile();
        }
    }
}
