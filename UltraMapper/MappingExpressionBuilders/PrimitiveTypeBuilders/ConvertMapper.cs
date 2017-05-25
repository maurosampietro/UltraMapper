using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ConvertMapper : PrimitiveMapperBase
    {
        private static Type _convertType = typeof( Convert );
        private static HashSet<TypePair> _supportedConversions = new HashSet<TypePair>();

        public ConvertMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            bool areTypesBuiltIn = source.IsBuiltInType( false ) &&
                target.IsBuiltInType( false );

            var isConvertible = new Lazy<bool>( () =>
            {
                try
                {
                    var typePair = new TypePair( source, target );
                    if( _supportedConversions.Contains( typePair ) )
                        return true;

                    if( !source.ImplementsInterface( typeof( IConvertible ) ) )
                        return false;

                    var testValue = Activator.CreateInstance( source );
                    Convert.ChangeType( testValue, target );

                    _supportedConversions.Add( typePair );
                    return true;
                }
                catch( InvalidCastException )
                {
                    return false;
                }
                catch( Exception )
                {
                    return false;
                }
            } );

            return areTypesBuiltIn || isConvertible.Value;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            var methodName = $"To{context.TargetInstance.Type.Name}";
            var methodParams = new[] { context.SourceInstance.Type };

            var convertMethod = _convertType.GetMethod( methodName, methodParams );
            return Expression.Call( convertMethod, context.SourceInstance );
        }
    }
}
