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
        public static bool VerifyMapperResult( this TypeMapper typeMapper, object source, object target )
        {
            return VerifyMapperResultHelper( typeMapper, source, target, new ReferenceTracking() );
        }

        private static bool VerifyMapperResultHelper( this TypeMapper typeMapper,
            object source, object target, ReferenceTracking referenceTracking )
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            if( !sourceType.IsValueType && source != null )
            {
                if( referenceTracking.Contains( source, targetType ) )
                    return true;

                if( !sourceType.IsValueType )
                    referenceTracking.Add( source, targetType, target );
            }

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
                    if( !VerifyMapperResultHelper( typeMapper, firstPos.Current, secondPos.Current, referenceTracking ) )
                        return false;

                    hasFirst = firstPos.MoveNext();
                    hasSecond = secondPos.MoveNext();
                }

                if( hasFirst ^ hasSecond )
                    return false;
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

                if( !mapping.SourceProperty.MemberType.IsValueType && sourceValue != null )
                {
                    if( referenceTracking.Contains( sourceValue, mapping.TargetProperty.MemberType ) )
                        continue;

                    referenceTracking.Add( sourceValue, mapping.TargetProperty.MemberType, targetValue );
                }

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
                        if( !VerifyMapperResultHelper( typeMapper, firstPos.Current, secondPos.Current, referenceTracking ) )
                            return false;

                        hasFirst = firstPos.MoveNext();
                        hasSecond = secondPos.MoveNext();
                    }

                    if( hasFirst ^ hasSecond )
                        return false;
                }

                //same value type: just compare their values
                if( sourceValue.GetType() == targetValue.GetType() && sourceValue.GetType().IsValueType )
                {
                    if( !sourceValue.Equals( targetValue ) )
                        return false;
                }
            }

            return true;
        }
    }
}
