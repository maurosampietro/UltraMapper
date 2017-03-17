using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeMapper.MappingConventions
{
    public class PropertyMatchingConfiguration : IEnumerable<IMatchingRule>
    {
        protected Dictionary<Type, IMatchingRule> _propertyMatchingRules;

        public Func<MemberInfo, MemberInfo, bool> MatchingEvaluator { get; set; }

        public PropertyMatchingConfiguration( Action<PropertyMatchingConfiguration> config )
                : this() { config?.Invoke( this ); }

        public PropertyMatchingConfiguration()
        {
            _propertyMatchingRules = new Dictionary<Type, IMatchingRule>();
            this.RespectAll();
        }

        public PropertyMatchingConfiguration<T> GetOrAdd<T>( 
            Action<T> ruleConfig = null ) where T : IMatchingRule, new()
        {
            var type = typeof( T );

            IMatchingRule instance;
            if( !_propertyMatchingRules.TryGetValue( type, out instance ) )
                _propertyMatchingRules.Add( type, instance = new T() );

            ruleConfig?.Invoke( (T)instance );
            return new PropertyMatchingConfiguration<T>( this );
        }

        public PropertyMatchingConfiguration Remove<T>() where T : IMatchingRule
        {
            _propertyMatchingRules.Remove( typeof( T ) );
            return this;
        }

        public void RespectAll()
        {
            MatchingEvaluator = new Func<MemberInfo, MemberInfo, bool>( ( source, target ) =>
               _propertyMatchingRules.Values.All( rule => rule.IsCompliant( source, target ) ) );
        }

        public void RespectAny()
        {
            MatchingEvaluator = new Func<MemberInfo, MemberInfo, bool>( ( source, target ) =>
               _propertyMatchingRules.Values.Any( rule => rule.IsCompliant( source, target ) ) );
        }

        public IEnumerator<IMatchingRule> GetEnumerator()
        {
            return _propertyMatchingRules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IMatchingRule this[ Type type ]
        {
            get { return _propertyMatchingRules[ type ]; }
        }
    }

    /*These helper classes carry generic type around
     *and allow static typed use of Respect method
     */   

    public class PropertyMatchingConfiguration<T1>
    {
        protected PropertyMatchingConfiguration _baseConfig;

        public PropertyMatchingConfiguration( PropertyMatchingConfiguration baseConfig )
        {
            _baseConfig = baseConfig;
        }

        public PropertyMatchingConfiguration( PropertyMatchingConfiguration<T1> baseConfig )
        {
            _baseConfig = baseConfig._baseConfig;
        }

        public PropertyMatchingConfiguration<T1, T2> GetOrAdd<T2>(
            Action<T2> ruleConfig = null ) where T2 : IMatchingRule, new()
        {
            _baseConfig.GetOrAdd( ruleConfig );
            return new PropertyMatchingConfiguration<T1, T2>( this );
        }
    }

    public class PropertyMatchingConfiguration<T1, T2> : PropertyMatchingConfiguration<T1>
    {
        public PropertyMatchingConfiguration( PropertyMatchingConfiguration<T1> baseConfig )
            : base( baseConfig ) { }

        new public PropertyMatchingConfiguration<T1, T2, T3> GetOrAdd<T3>(
            Action<T3> ruleConfig = null ) where T3 : IMatchingRule, new()
        {
            _baseConfig.GetOrAdd( ruleConfig );
            return new PropertyMatchingConfiguration<T1, T2, T3>( this );
        }

        public void Respect( Func<T1, T2, RuleChaining> matchCondition )
        {
            var t1Instance = (T1)_baseConfig[ typeof( T1 ) ];
            var t2instance = (T2)_baseConfig[ typeof( T2 ) ];

            _baseConfig.MatchingEvaluator = matchCondition( t1Instance,
                t2instance ).Expression.Compile();
        }
    }

    public class PropertyMatchingConfiguration<T1, T2, T3> : PropertyMatchingConfiguration<T1, T2>
    {
        public PropertyMatchingConfiguration( PropertyMatchingConfiguration<T1, T2> baseConfig )
            : base( baseConfig ) { }

        new public PropertyMatchingConfiguration<T1, T2, T3, T4> GetOrAdd<T4>(
            Action<T4> ruleConfig = null ) where T4 : IMatchingRule, new()
        {
            _baseConfig.GetOrAdd( ruleConfig );
            return new PropertyMatchingConfiguration<T1, T2, T3, T4>( this );
        }

        public void Respect( Func<T1, T2, T3, RuleChaining> matchCondition )
        {
            var t1Instance = (T1)_baseConfig[ typeof( T1 ) ];
            var t2instance = (T2)_baseConfig[ typeof( T2 ) ];
            var t3instance = (T3)_baseConfig[ typeof( T3 ) ];

            _baseConfig.MatchingEvaluator = matchCondition( t1Instance,
                 t2instance, t3instance ).Expression.Compile();
        }
    }

    public class PropertyMatchingConfiguration<T1, T2, T3, T4> : PropertyMatchingConfiguration<T1, T2, T3>
    {
        public PropertyMatchingConfiguration( PropertyMatchingConfiguration<T1, T2, T3> baseConfig )
            : base( baseConfig ) { }

        new public PropertyMatchingConfiguration<T1, T2, T3, T4> GetOrAdd<T5>(
            Action<T5> ruleConfig = null ) where T5 : IMatchingRule, new()
        {
            _baseConfig.GetOrAdd( ruleConfig );
            return this;
        }

        public void Respect( Func<T1, T2, T3, T4, RuleChaining> matchCondition )
        {
            var t1Instance = (T1)_baseConfig[ typeof( T1 ) ];
            var t2instance = (T2)_baseConfig[ typeof( T2 ) ];
            var t3instance = (T3)_baseConfig[ typeof( T3 ) ];
            var t4Instance = (T4)_baseConfig[ typeof( T4 ) ];

            _baseConfig.MatchingEvaluator = matchCondition( t1Instance,
                t2instance, t3instance, t4Instance ).Expression.Compile();
        }
    }
}
