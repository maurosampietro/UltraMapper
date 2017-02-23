using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public sealed class BuiltInTypeMapper : IObjectMapperExpression, IMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            return this.CanHandle( sourcePropertyType, targetPropertyType );
        }

        public bool CanHandle( Type source, Type target )
        {
            bool areTypesBuiltIn = source.IsBuiltInType( false )
                && target.IsBuiltInType( false );

            return (areTypesBuiltIn) && (source == target ||
                    source.IsImplicitlyConvertibleTo( target ) ||
                    source.IsExplicitlyConvertibleTo( target ));
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

                var conversionExp = Expression.Convert(
                    readValueExp, targetPropertyType );

                return Expression.Assign( value, conversionExp );
            };

            Expression valueAssignment = getValueAssignmentExp();
            
            //var mapExp= this.GetMappingExpression( sourcePropertyType, targetPropertyType );
            //var readValueExp = mapping.SourceProperty.ValueGetter.Body;
            //Expression valueAssignment = Expression.Assign( value,
            //    Expression.Invoke( mapExp, readValueExp ) );

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

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
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
