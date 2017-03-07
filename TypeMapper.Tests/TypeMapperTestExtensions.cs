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
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            //sharing the same reference or both null
            if( Object.ReferenceEquals( source, target ) )
                return true;

            //either source or target is null 
            if( source == null || target == null )
                return false;

            //same value type: just compare their values
            if( sourceType == targetType && sourceType.IsValueType )
                return source.Equals( target );

            if( sourceType.IsEnumerable() && targetType.IsEnumerable() )
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

            var typeMapping = typeMapper.MappingConfiguration[
                source.GetType(), target.GetType() ];

            foreach( var mapping in typeMapping.MemberMappings.Values )
            {
                var sourceValue = mapping.SourceProperty
                    .ValueGetter.Compile().DynamicInvoke( source );

                var converter = mapping.CustomConverter;
                if( converter != null )
                    sourceValue = converter.Compile().DynamicInvoke( sourceValue );

                var targetValue = mapping.TargetProperty
                    .ValueGetter.Compile().DynamicInvoke( target );

                if( Object.ReferenceEquals( sourceValue, targetValue ) )
                    continue;

                if( mapping.SourceProperty.IsNullable &&
                    !mapping.TargetProperty.IsNullable && sourceValue == null
                    && targetValue.Equals( mapping.TargetProperty.MemberType.GetDefaultValueViaActivator() ) )
                    continue;

                if( sourceValue == null ^ targetValue == null )
                    return false;

                if( sourceValue.GetType().IsEnumerable() && targetValue.GetType().IsEnumerable() )
                {
                    var firstPos = (sourceValue as IEnumerable).GetEnumerator();
                    var secondPos = (targetValue as IEnumerable).GetEnumerator();

                    var hasFirst = firstPos.MoveNext();
                    var hasSecond = secondPos.MoveNext();

                    while( hasFirst && hasSecond )
                    {
                        if( !VerifyMapperResult( typeMapper, firstPos.Current, secondPos.Current ) )
                            return false;

                        hasFirst = firstPos.MoveNext();
                        hasSecond = secondPos.MoveNext();
                    }

                    if( hasFirst ^ hasSecond )
                        return false;
                }
                else if( !verify( sourceValue, targetValue ) )
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
