using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    internal static class ExpressionLoops
    {
        public static Expression ForEach( Expression collection, ParameterExpression loopVar, Expression loopContent )
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof( IEnumerable<> ).MakeGenericType( elementType );
            var enumeratorType = typeof( IEnumerator<> ).MakeGenericType( elementType );

            var enumeratorVar = Expression.Variable( enumeratorType, "enumerator" );
            var getEnumeratorCall = Expression.Call( collection, enumerableType.GetMethod( "GetEnumerator" ) );
            var enumeratorAssign = Expression.Assign( enumeratorVar, getEnumeratorCall );

            //The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call( enumeratorVar, typeof( IEnumerator )
                .GetMethod( nameof( IEnumerator.MoveNext ) ) );

            var breakLabel = Expression.Label( "LoopBreak" );

            return Expression.Block
            (
                new[] { enumeratorVar },

                enumeratorAssign,

                Expression.Loop
                (
                    Expression.IfThenElse
                    (
                        Expression.Equal( moveNextCall, Expression.Constant( true ) ),
                        Expression.Block
                        (
                            new[] { loopVar },

                            Expression.Assign( loopVar, Expression.Property( enumeratorVar, "Current" ) ),
                            loopContent
                        ),

                        Expression.Break( breakLabel )
                    ),

                    breakLabel
               ) );
        }

        public static Expression For( ParameterExpression loopVar, Expression initValue,
            Expression condition, Expression increment, Expression loopContent )
        {
            var initAssign = Expression.Assign( loopVar, initValue );
            var breakLabel = Expression.Label( "LoopBreak" );

            var loop = Expression.Block
            (
                new[] { loopVar },

                initAssign,

                Expression.Loop
                (
                    Expression.IfThenElse
                    (
                        condition,
                        Expression.Block
                        (
                            loopContent,
                            increment
                        ),

                        Expression.Break( breakLabel )
                    ),

                    breakLabel
                )
            );

            return loop;
        }
    }
}
