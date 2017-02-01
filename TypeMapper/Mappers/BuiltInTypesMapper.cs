using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public sealed class BuiltInTypeMapper : IObjectMapperExpression, ITypeMapperExpression
    {
        public bool CanHandle( TypeMapping mapping )
        {
            var sourceType = mapping.TypePair.SourceType;
            var targetType = mapping.TypePair.TargetType;

            bool areTypesBuiltIn = sourceType.IsBuiltInType( false ) &&
                targetType.IsBuiltInType( false );

            bool areSameTypeOrConvertible = (sourceType == targetType ||
                    sourceType.IsImplicitlyConvertibleTo( targetType ) ||
                    sourceType.IsExplicitlyConvertibleTo( targetType ));

            return areTypesBuiltIn && areSameTypeOrConvertible;
        }

        public bool CanHandle( MemberMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            bool areTypesBuiltIn = mapping.SourceProperty.IsBuiltInType &&
                mapping.TargetProperty.IsBuiltInType;

            return (areTypesBuiltIn) && (sourcePropertyType == targetPropertyType ||
                    sourcePropertyType.IsImplicitlyConvertibleTo( targetPropertyType ) ||
                    sourcePropertyType.IsExplicitlyConvertibleTo( targetPropertyType ));
        }

        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            //Action<sourceType, targetType>

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

                if( sourcePropertyType == targetPropertyType )
                    return Expression.Assign( value, readValueExp );

                try
                {
                    return Expression.Assign( value, Expression.Convert(
                        readValueExp, targetPropertyType ) );
                }
                catch( Exception ex )
                {
                    throw new Exception( $"Cannot handle {mapping}", ex );
                }
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

        public LambdaExpression GetMappingExpression( TypeMapping mapping )
        {
            //Func<sourceType, targetType>

            var sourceType = mapping.TypePair.SourceType;
            var targetType = mapping.TypePair.TargetType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );

            var value = Expression.Variable( targetType, "value" );

            Func<Expression> getValueExp = () =>
            {
                if( sourceType == targetType )
                    return Expression.Assign( value, sourceInstance );

                var conversionExp = Expression.Convert(
                    sourceInstance, targetType );

                return Expression.Assign( value, conversionExp );
            };

            var body = Expression.Block( new[] { value }, getValueExp() );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType, body, sourceInstance );
        }
    }
}
