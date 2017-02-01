using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class NullableMapper : IObjectMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            return mapping.SourceProperty.IsNullable ||
                mapping.TargetProperty.IsNullable;
        }

        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            var sourceType = mapping.SourceProperty.MemberInfo.ReflectedType;
            var targetType = mapping.TargetProperty.MemberInfo.ReflectedType;

            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var value = Expression.Variable( targetPropertyType, "value" );

            Func<Expression> getValueAssignmentExp = () =>
            {
                var readValueExp = mapping.SourceProperty.ValueGetter.Body;

                if( mapping.CustomConverter != null )
                    return Expression.Invoke( mapping.CustomConverter, readValueExp );

                if( sourcePropertyType == targetPropertyType )
                    return Expression.Assign( value, readValueExp );

                if( mapping.SourceProperty.IsNullable && !mapping.TargetProperty.IsNullable )
                {
                    var nullableValueAccess = Expression.MakeMemberAccess( readValueExp, sourcePropertyType.GetProperty( "Value" ) );

                    var sourceUnderlyingType = mapping.SourceProperty.NullableUnderlyingType;
                    if( sourceUnderlyingType == targetPropertyType )
                    {
                        return Expression.IfThenElse
                        (
                            Expression.Equal( readValueExp, Expression.Constant( null, sourcePropertyType ) ),
                            Expression.Assign( value, Expression.Default( targetPropertyType ) ),
                            Expression.Assign( value, nullableValueAccess )
                        );
                    }

                    if( sourceUnderlyingType.IsImplicitlyConvertibleTo( targetPropertyType ) ||
                        sourceUnderlyingType.IsExplicitlyConvertibleTo( targetPropertyType ) )
                    {
                        return Expression.IfThenElse
                        (
                            Expression.Equal( readValueExp, Expression.Constant( null, sourcePropertyType ) ),
                            Expression.Assign( value, Expression.Default( targetPropertyType ) ),
                            Expression.Assign( value, Expression.Convert( nullableValueAccess, targetPropertyType ) )
                        );
                    }

                    var convertMethod = typeof( Convert ).GetMethod( $"To{targetPropertyType.Name}", new[] { sourcePropertyType } );
                    return Expression.IfThenElse
                    (
                        Expression.Equal( readValueExp, Expression.Constant( null, sourcePropertyType ) ),
                        Expression.Assign( value, Expression.Default( targetPropertyType ) ),
                        Expression.Assign( value, Expression.Call( convertMethod, nullableValueAccess ) )
                    );
                }

                if( !mapping.SourceProperty.IsNullable && mapping.TargetProperty.IsNullable )
                {
                    var targetUnderlyingType = mapping.TargetProperty.NullableUnderlyingType;

                    if( sourcePropertyType.IsImplicitlyConvertibleTo( targetUnderlyingType ) ||
                      sourcePropertyType.IsExplicitlyConvertibleTo( targetUnderlyingType ) )
                    {
                        var constructor = targetPropertyType.GetConstructor( new Type[] { targetUnderlyingType } );
                        var newNullable = Expression.New( constructor, Expression.Convert( readValueExp, targetUnderlyingType ) );
                        return Expression.Assign( value, newNullable );
                    }
                    else
                    {
                        var constructor = targetPropertyType.GetConstructor( new Type[] { sourcePropertyType } );
                        var newNullable = Expression.New( constructor, readValueExp );
                        return Expression.Assign( value, newNullable );
                    }
                }

                throw new Exception( $"Cannot handle {mapping}" );
            };

            Expression valueAssignment = getValueAssignmentExp();

            var setValueExp = (Expression)Expression.Block
            (
                new[] { value },

                valueAssignment.ReplaceParameter( sourceInstance ),
                mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( targetInstance, "target" )
                    .ReplaceParameter( value, "value" )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType );

            return Expression.Lambda( delegateType, setValueExp, 
                referenceTrack, sourceInstance, targetInstance );
        }
    }
}
