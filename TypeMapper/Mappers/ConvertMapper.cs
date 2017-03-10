using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ConvertMapper : BaseMapper, IMemberMappingMapperExpression, ITypeMappingMapperExpression
    {
        private static Type _convertType = typeof( Convert );

        public override bool CanHandle( Type source, Type target )
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

        protected override Expression GetTargetValueAssignment( MapperContext context )
        {
            var methodName = $"To{context.TargetMemberType.Name}";
            var methodParams = new[] { context.SourceMemberType };

            var convertMethod = _convertType.GetMethod( methodName, methodParams );
            var convertMethodCall = Expression.Call( convertMethod, context.SourceMemberValue );

            return Expression.Assign( context.TargetMember, convertMethodCall );
        }
    }
}
