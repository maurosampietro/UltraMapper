using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class NullableMapper : BaseMapper, IObjectMapperExpression, IMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            var sourceValueType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetValueType = mapping.TargetProperty.MemberInfo.GetMemberType();

            return this.CanHandle( sourceValueType, targetValueType );
        }

        public bool CanHandle( Type source, Type target )
        {
            return source.IsNullable() || target.IsNullable();
        }

        protected override Expression GetValueAssignment( MapperContext context )
        {
            if( context.SourceValueType == context.TargetValueType )
                return Expression.Assign( context.TargetValue, context.SourceValue );

            if( context.SourceValueType.IsNullable() && context.TargetValueType.IsNullable() )
            {
                var sourceUnderlyingType = Nullable.GetUnderlyingType( context.SourceValueType );
                var targetUnderlyingType = Nullable.GetUnderlyingType( context.TargetValueType );

                //Nullable<int> is used only because it is forbidden to use nameof with open generics.
                //Any other struct type instead of int would work.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceInstance,
                    context.SourceValueType.GetProperty( nameof( Nullable<int>.Value ) ) );

                var conversion = MappingExpressionBuilderFactory.GetMappingExpression(
                    sourceUnderlyingType, targetUnderlyingType );

                var constructor = context.TargetValueType.GetConstructor( new Type[] { targetUnderlyingType } );
                var newNullable = Expression.New( constructor, Expression.Invoke( conversion, nullableValueAccess ) );

                return Expression.IfThenElse
                (
                    Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceValueType ) ),
                    Expression.Assign( context.TargetValue, Expression.Default( context.TargetValueType ) ),
                    Expression.Assign( context.TargetValue, newNullable )
                );
            }

            if( context.SourceValueType.IsNullable() && !context.TargetValueType.IsNullable() )
            {
                //Nullable<int> is used only because it is forbidden to use nameof with open generics.
                //Any other struct type instead of int would work.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceValue,
                    context.SourceValueType.GetProperty( nameof( Nullable<int>.Value ) ) );

                var sourceUnderlyingType = context.SourceValueType.GetUnderlyingTypeIfNullable();
                if( sourceUnderlyingType == context.TargetValueType )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceValue, Expression.Constant( null, context.SourceValueType ) ),
                        Expression.Assign( context.TargetValue, Expression.Default( context.TargetValueType ) ),
                        Expression.Assign( context.TargetValue, nullableValueAccess )
                    );
                }

                if( sourceUnderlyingType.IsImplicitlyConvertibleTo( context.TargetValueType ) ||
                    sourceUnderlyingType.IsExplicitlyConvertibleTo( context.TargetValueType ) )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceValue, Expression.Constant( null, context.SourceValueType ) ),
                        Expression.Assign( context.TargetValue, Expression.Default( context.TargetValueType ) ),
                        Expression.Assign( context.TargetValue, Expression.Convert( nullableValueAccess, context.TargetValueType ) )
                    );
                }

                var convertMethod = typeof( Convert ).GetMethod( $"To{context.TargetValueType.Name}", new[] { context.SourceValueType } );
                return Expression.IfThenElse
                (
                    Expression.Equal( context.SourceValue, Expression.Constant( null, context.SourceValueType ) ),
                    Expression.Assign( context.TargetValue, Expression.Default( context.TargetValueType ) ),
                    Expression.Assign( context.TargetValue, Expression.Call( convertMethod, nullableValueAccess ) )
                );
            }

            if( !context.SourceValueType.IsNullable() && context.TargetValueType.IsNullable() )
            {
                var targetUnderlyingType = context.TargetValueType.GetUnderlyingTypeIfNullable();

                if( context.SourceValueType.IsImplicitlyConvertibleTo( targetUnderlyingType ) ||
                  context.SourceValueType.IsExplicitlyConvertibleTo( targetUnderlyingType ) )
                {
                    var constructor = context.TargetValueType.GetConstructor( new Type[] { targetUnderlyingType } );
                    var newNullable = Expression.New( constructor, Expression.Convert( context.SourceValue, targetUnderlyingType ) );
                    return Expression.Assign( context.TargetValue, newNullable );
                }
                else
                {
                    var constructor = context.TargetValueType.GetConstructor( new Type[] { context.SourceValueType } );
                    var newNullable = Expression.New( constructor, context.SourceValue );
                    return Expression.Assign( context.TargetValue, newNullable );
                }
            }

            throw new Exception( $"Cannot handle {context.SourceValue} -> {context.TargetValue}" );
        }
    }
}
