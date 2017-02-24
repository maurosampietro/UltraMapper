using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ConvertMapper : BaseMapper, IObjectMapperExpression, IMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            return CanHandle( sourcePropertyType, targetPropertyType );
        }

        public bool CanHandle( Type source, Type target )
        {
            bool areTypesBuiltIn = source.IsBuiltInType( false ) &&
                target.IsBuiltInType( false );

            var isConvertible = new Lazy<bool>( () =>
            {
                try
                {
                    if( !source.ImplementsInterface( typeof( IConvertible ) ) )
                        return false;

                    //reference types are ok but if mapping to the same 
                    //type a referencemapper should be used
                    if( source == target )
                        return false;

                    var testValue = InstanceFactory.CreateObject( source );
                    Convert.ChangeType( testValue, target );

                    return true;
                }
                catch( InvalidCastException )
                {
                    return false;
                }
                catch( Exception ex )
                {
                    return false;
                }
            } );

            return areTypesBuiltIn || isConvertible.Value;
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            //Func<SourceType, TargetType>

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var value = Expression.Variable( targetType, "value" );

            var convertMethod = typeof( Convert ).GetMethod(
                $"To{targetType.Name}", new[] { sourceType } );

            var conversionExp = Expression.Call( convertMethod, sourceInstance );
            var valueAssignment = Expression.Assign( value, conversionExp );
            var body = Expression.Block( new[] { value }, valueAssignment );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType, body,
                sourceInstance );
        }

        protected override Expression GetValueAssignment( MapperContext context )
        {
            var convertMethod = typeof( Convert ).GetMethod(
                $"To{context.TargetPropertyType.Name}", new[] { context.SourcePropertyType } );

            var sourceGetterInstanceParamName = context.Mapping.SourceProperty
                .ValueGetter.Parameters[ 0 ].Name;

            var readValueExp = context.Mapping.SourceProperty.ValueGetter.Body
                    .ReplaceParameter( context.SourceInstance, sourceGetterInstanceParamName );

            return Expression.Assign( context.TargetValue,
                Expression.Call( convertMethod, readValueExp ) );
        }
    }
}
