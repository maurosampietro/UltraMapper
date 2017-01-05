using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Tests
{
    public static class TypeMapperTestExtensions
    {
        public static bool VerifyMapperResult( this TypeMapper typeMapper,
            object source, object target, Func<object, object, bool> verify )
        {
            if( Object.ReferenceEquals( source, target ) )
                return true;

            if( source == null || target == null )
                return false;

            if( source.GetType().IsEnumerable() && target.GetType().IsEnumerable() )
            {
                if( source.GetType() == target.GetType() )
                {
                    var firstPos = (source as IEnumerable).GetEnumerator();
                    var secondPos = (target as IEnumerable).GetEnumerator();

                    var hasFirst = firstPos.MoveNext();
                    var hasSecond = secondPos.MoveNext();

                    while( hasFirst && hasSecond )
                    {
                        if( !Equals( firstPos.Current, secondPos.Current ) )
                            return false;

                        hasFirst = firstPos.MoveNext();
                        hasSecond = secondPos.MoveNext();
                    }

                    return !hasFirst && !hasSecond;
                }
                else
                {
                    var targetColl = (target as IEnumerable);
                    foreach( var sVal in source as IEnumerable )
                    {
                        if( !targetColl.Cast<object>().Contains( sVal ) )
                            return false;
                    }
                }
            }

            var typeMapping = typeMapper.MappingConfiguration[
                source.GetType(), target.GetType() ];

            foreach( var mapping in typeMapping.PropertyMappings.Select( m => m.Value ) )
            {
                var sourceValue = mapping.SourceProperty
                    .PropertyInfo.GetValue( source );

                var converter = mapping.CustomConverter;
                if( converter != null )
                    sourceValue = converter.Compile().DynamicInvoke( sourceValue );

                var destinationValue = mapping.TargetProperty
                    .PropertyInfo.GetValue( target );

                if( !verify( sourceValue, destinationValue ) )
                    return false;
            }

            return true;
        }

        public static bool VerifyMapperResult( this TypeMapper typeMapper, object source, object target )
        {
            return VerifyMapperResult( typeMapper, source, target, ( sourceValue, destinationValue ) =>
            {
                if( Object.ReferenceEquals( sourceValue, destinationValue ) )
                    return true;

                if( sourceValue.GetType().IsBuiltInType( false ) )
                    sourceValue.Equals( destinationValue );

                return typeMapper.VerifyMapperResult( sourceValue, destinationValue );
            } );
        }
    }
}
