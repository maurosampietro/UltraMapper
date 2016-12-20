using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ConvertMapper : IObjectMapperExpression
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            bool areTypesBuiltIn = mapping.SourceProperty.IsBuiltInType &&
                mapping.TargetProperty.IsBuiltInType;

            var isConvertible = new Lazy<bool>( () =>
            {
                try
                {
                    if( !sourcePropertyType.ImplementsInterface( typeof( IConvertible ) ) )
                        return false;

                    //reference types are ok but if mapping to the same 
                    //type a referencemapper should be used
                    if( sourcePropertyType == targetPropertyType )
                        return false;

                    var testValue = InstanceFactory.CreateObject( sourcePropertyType );
                    Convert.ChangeType( testValue, targetPropertyType );

                    return true;
                }
                catch( InvalidCastException )
                {
                    return false;
                }
            } );

            return areTypesBuiltIn || isConvertible.Value;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Action<ReferenceTracking, sourceType, targetType>

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var value = Expression.Variable( targetPropertyType, "value" );
            var convertMethod = typeof( Convert ).GetMethod( $"To{targetPropertyType.Name}",
                new[] { sourcePropertyType } );

            var setValueExp = (Expression)Expression.Block
            (
                new[] { value },

                Expression.Call( convertMethod, mapping.SourceProperty.ValueGetter.Body )
                    .ReplaceParameter( sourceInstance ),

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
