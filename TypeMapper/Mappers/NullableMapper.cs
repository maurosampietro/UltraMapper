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
            return mapping.SourceProperty.IsNullable ||
                mapping.TargetProperty.IsNullable;
        }

        public bool CanHandle( Type source, Type target )
        {
            return source.IsNullable() || target.IsNullable();
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );

            var value = Expression.Variable( targetType, "value" );

            Func<Expression> getValueAssignmentExp = () =>
            {
                if( sourceType == targetType )
                    return Expression.Assign( value, sourceInstance );

                if( sourceType.IsNullable() && targetType.IsNullable() )
                {
                    var sourceUnderlyingType = Nullable.GetUnderlyingType( sourceType );
                    var targetUnderlyingType = Nullable.GetUnderlyingType( targetType );

                    var nullableValueAccess = Expression.MakeMemberAccess( sourceInstance,
                        sourceType.GetProperty( "Value" ) );

                    var conversion = MappingExpressionBuilderFactory.GetMappingExpression(
                        sourceUnderlyingType, targetUnderlyingType );

                    var constructor = targetType.GetConstructor( new Type[] { targetUnderlyingType } );
                    var newNullable = Expression.New( constructor, Expression.Invoke( conversion, nullableValueAccess ) );

                    return Expression.IfThenElse
                    (
                        Expression.Equal( sourceInstance, Expression.Constant( null, sourceType ) ),
                        Expression.Assign( value, Expression.Default( targetType ) ),
                        Expression.Assign( value, newNullable )
                    );
                }

                if( sourceType.IsNullable() && !targetType.IsNullable() )
                {
                    var sourceUnderlyingType = Nullable.GetUnderlyingType( sourceType );

                    var conversion = MappingExpressionBuilderFactory.GetMappingExpression(
                        sourceUnderlyingType, targetType );

                    var nullableValueAccess = Expression.MakeMemberAccess( sourceInstance,
                        sourceType.GetProperty( "Value" ) );

                    return Expression.IfThenElse
                    (
                        Expression.Equal( sourceInstance, Expression.Constant( null, sourceType ) ),
                        Expression.Assign( value, Expression.Default( targetType ) ),
                        Expression.Assign( value, Expression.Invoke( conversion, nullableValueAccess ) )
                    );
                }

                if( !sourceType.IsNullable() && targetType.IsNullable() )
                {
                    var targetUnderlyingType = Nullable.GetUnderlyingType( targetType );

                    var conversion = MappingExpressionBuilderFactory.GetMappingExpression(
                        sourceType, targetUnderlyingType );

                    var constructor = targetType.GetConstructor( new Type[] { sourceType } );
                    var newNullable = Expression.New( constructor, Expression.Invoke( conversion, sourceInstance ) );

                    return Expression.Assign( value, newNullable );
                }

                throw new Exception( $"Cannot handle {sourceType} -> {targetType}" );
            };

            Expression valueAssignment = getValueAssignmentExp();

            var body = Expression.Block( new[] { value }, valueAssignment, value );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType, body, sourceInstance );
        }

        protected override Expression GetValueAssignment( MapperContext context )
        {
            var sourceGetterInstanceParamName = context.Mapping.SourceProperty
                .ValueGetter.Parameters[ 0 ].Name;

            var readValueExp = context.Mapping.SourceProperty.ValueGetter.Body
                    .ReplaceParameter( context.SourceInstance, sourceGetterInstanceParamName );

            if( context.SourcePropertyType == context.TargetPropertyType )
                return Expression.Assign( context.TargetValue, readValueExp );

            if( context.Mapping.SourceProperty.IsNullable && !context.Mapping.TargetProperty.IsNullable )
            {
                var nullableValueAccess = Expression.MakeMemberAccess( readValueExp,
                    context.SourcePropertyType.GetProperty( "Value" ) );

                var sourceUnderlyingType = context.Mapping.SourceProperty.NullableUnderlyingType;
                if( sourceUnderlyingType == context.TargetPropertyType )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( readValueExp, Expression.Constant( null, context.SourcePropertyType ) ),
                        Expression.Assign( context.TargetValue, Expression.Default( context.TargetPropertyType ) ),
                        Expression.Assign( context.TargetValue, nullableValueAccess )
                    );
                }

                if( sourceUnderlyingType.IsImplicitlyConvertibleTo( context.TargetPropertyType ) ||
                    sourceUnderlyingType.IsExplicitlyConvertibleTo( context.TargetPropertyType ) )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( readValueExp, Expression.Constant( null, context.SourcePropertyType ) ),
                        Expression.Assign( context.TargetValue, Expression.Default( context.TargetPropertyType ) ),
                        Expression.Assign( context.TargetValue, Expression.Convert( nullableValueAccess, context.TargetPropertyType ) )
                    );
                }

                var convertMethod = typeof( Convert ).GetMethod( $"To{context.TargetPropertyType.Name}", new[] { context.SourcePropertyType } );
                return Expression.IfThenElse
                (
                    Expression.Equal( readValueExp, Expression.Constant( null, context.SourcePropertyType ) ),
                    Expression.Assign( context.TargetValue, Expression.Default( context.TargetPropertyType ) ),
                    Expression.Assign( context.TargetValue, Expression.Call( convertMethod, nullableValueAccess ) )
                );
            }

            if( !context.Mapping.SourceProperty.IsNullable && context.Mapping.TargetProperty.IsNullable )
            {
                var targetUnderlyingType = context.Mapping.TargetProperty.NullableUnderlyingType;

                if( context.SourcePropertyType.IsImplicitlyConvertibleTo( targetUnderlyingType ) ||
                  context.SourcePropertyType.IsExplicitlyConvertibleTo( targetUnderlyingType ) )
                {
                    var constructor = context.TargetPropertyType.GetConstructor( new Type[] { targetUnderlyingType } );
                    var newNullable = Expression.New( constructor, Expression.Convert( readValueExp, targetUnderlyingType ) );
                    return Expression.Assign( context.TargetValue, newNullable );
                }
                else
                {
                    var constructor = context.TargetPropertyType.GetConstructor( new Type[] { context.SourcePropertyType } );
                    var newNullable = Expression.New( constructor, readValueExp );
                    return Expression.Assign( context.TargetValue, newNullable );
                }
            }

            throw new Exception( $"Cannot handle {context.Mapping}" );
        }
    }
}
