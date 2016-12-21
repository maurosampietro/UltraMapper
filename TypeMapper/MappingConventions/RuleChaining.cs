using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    public static class PropertyMatchingRuleChainingExtensions
    {
        private static readonly Type _propertyMatchingRuleType = typeof( IPropertyMatchingRule );
        private static readonly Type _propertInfoType = typeof( PropertyInfo );

        private static readonly ParameterExpression source = Expression.Parameter( _propertInfoType, "rule" );
        private static readonly ParameterExpression target = Expression.Parameter( _propertInfoType, "otherRule" );

        private static readonly MethodInfo _methodInfo = _propertyMatchingRuleType.GetMethod(
            nameof( IPropertyMatchingRule.IsCompliant ), new Type[] { _propertInfoType, _propertInfoType } );

        #region And (&&)
        public static RuleChaining And( this IPropertyMatchingRule rule, IPropertyMatchingRule otherRule )
        {
            var checkRule = Expression.Call( Expression.Constant( rule ), _methodInfo, source, target );
            var checkOtherRule = Expression.Call( Expression.Constant( otherRule ), _methodInfo, source, target );

            return new RuleChaining() { Expression = checkRule.And( checkOtherRule ) };
        }

        public static RuleChaining And( this IPropertyMatchingRule rule, RuleChaining innerCheck )
        {
            var checkRule = Expression.Call( Expression.Constant( rule ), _methodInfo, source, target );
            return new RuleChaining() { Expression = checkRule.And( Expression.Invoke( innerCheck.Expression, source, target ) ) };
        }

        public static RuleChaining And( this RuleChaining rule, IPropertyMatchingRule innerCheck )
        {
            var checkOtherRule = Expression.Call( Expression.Constant( innerCheck ), _methodInfo, source, target );
            return new RuleChaining() { Expression = rule.Expression.And( checkOtherRule ) };
        }

        public static RuleChaining And( this RuleChaining rule, RuleChaining innerCheck )
        {
            return new RuleChaining() { Expression = rule.Expression.And( innerCheck.Expression ) };
        }

        private static Expression<Func<PropertyInfo, PropertyInfo, bool>> And( this Expression rule, Expression otherRule )
        {
            var lambda = rule as LambdaExpression;
            if( lambda != null )
            {
                var expBody = Expression.AndAlso( lambda.Body, otherRule );
                return Expression.Lambda<Func<PropertyInfo, PropertyInfo, bool>>( expBody, source, target );
            }

            var expBody2 = Expression.AndAlso( rule, otherRule );
            return Expression.Lambda<Func<PropertyInfo, PropertyInfo, bool>>( expBody2, source, target );
        }
        #endregion

        #region Or (||)
        public static RuleChaining Or( this IPropertyMatchingRule rule, IPropertyMatchingRule otherRule )
        {
            var checkRule = Expression.Call( Expression.Constant( rule ), _methodInfo, source, target );
            var checkOtherRule = Expression.Call( Expression.Constant( otherRule ), _methodInfo, source, target );

            return new RuleChaining() { Expression = checkRule.Or( checkOtherRule ) };
        }

        public static RuleChaining Or( this IPropertyMatchingRule rule, RuleChaining innerCheck )
        {
            var checkRule = Expression.Call( Expression.Constant( rule ), _methodInfo, source, target );
            return new RuleChaining() { Expression = checkRule.Or( Expression.Invoke( innerCheck.Expression, source, target ) ) };
        }

        public static RuleChaining Or( this RuleChaining rule, IPropertyMatchingRule innerCheck )
        {
            var checkOtherRule = Expression.Call( Expression.Constant( innerCheck ), _methodInfo, source, target );
            return new RuleChaining() { Expression = rule.Expression.Or( checkOtherRule ) };
        }

        public static RuleChaining Or( this RuleChaining rule, RuleChaining innerCheck )
        {
            return new RuleChaining() { Expression = rule.Expression.Or( innerCheck.Expression ) };
        }

        private static Expression<Func<PropertyInfo, PropertyInfo, bool>> Or( this Expression rule, Expression otherRule )
        {
            var lambda = rule as LambdaExpression;
            if( lambda != null )
            {
                var expBody = Expression.OrElse( lambda.Body, otherRule );
                return Expression.Lambda<Func<PropertyInfo, PropertyInfo, bool>>( expBody, source, target );
            }

            var expBody2 = Expression.OrElse( rule, otherRule );
            return Expression.Lambda<Func<PropertyInfo, PropertyInfo, bool>>( expBody2, source, target );
        }
        #endregion
    }

    public class RuleChaining
    {
        public Expression<Func<PropertyInfo, PropertyInfo, bool>> Expression { get; set; }

        public static RuleChaining operator &( RuleChaining lhs, RuleChaining rhs )
        {
            return lhs.And( rhs );
        }

        //public static RuleChaining operator &( RuleChaining lhs, IPropertyMatchingRule rhs )
        //{
        //    return lhs.And( rhs );
        //}

        public static RuleChaining operator |( RuleChaining lhs, RuleChaining rhs )
        {
            return lhs.Or( rhs );
        }

        //public static RuleChaining operator |( RuleChaining lhs, IPropertyMatchingRule rhs )
        //{
        //    return lhs.Or( rhs );
        //}

        public static bool operator true( RuleChaining rhs )
        {
            return true;
        }

        public static bool operator false( RuleChaining rhs )
        {
            return false;
        }
    }
}
