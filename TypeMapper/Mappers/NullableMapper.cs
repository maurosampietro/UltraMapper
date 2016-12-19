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
        public bool CanHandle( PropertyMapping mapping )
        {
            return mapping.SourceProperty.IsNullable ||
                mapping.TargetProperty.IsNullable;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

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
                    return Expression.IfThenElse
                    (
                        Expression.Equal( readValueExp, Expression.Constant( null, sourcePropertyType ) ),
                        Expression.Assign( value, Expression.Default( targetPropertyType ) ),
                        Expression.Assign( value, Expression.MakeMemberAccess( readValueExp, sourcePropertyType.GetProperty( "Value" ) ) )
                    );
                }

                if( !mapping.SourceProperty.IsNullable && mapping.TargetProperty.IsNullable )
                {
                    var constructor = targetPropertyType.GetConstructor( new Type[] { sourcePropertyType } );
                    return Expression.New( constructor, readValueExp );
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

            return Expression.Lambda( delegateType,
                setValueExp, referenceTrack, sourceInstance, targetInstance );
        }
    }
}
