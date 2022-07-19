using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public sealed class ConvertMapper : PrimitiveMapperBase
    {
        private static readonly Type _convertType = typeof( Convert );
        private static readonly HashSet<TypePair> _supportedConversions = new HashSet<TypePair>();

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;
            bool areTypesBuiltIn = source.EntryType.IsBuiltIn( false ) && target.EntryType.IsBuiltIn( false );
            return areTypesBuiltIn || SourceIsConvertibleToTarget( source.EntryType, target.EntryType );
        }

        private bool SourceIsConvertibleToTarget( Type source, Type target )
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
