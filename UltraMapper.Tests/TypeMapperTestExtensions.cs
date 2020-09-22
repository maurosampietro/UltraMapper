using System;
using System.Collections;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    public static class TypeMapperTestExtensions
    {
        public static bool VerifyMapperResult( this Mapper ultraMapper, object source, object target )
        {
            return VerifyMapperResultHelper( ultraMapper, source, target, new ReferenceTracker() );
        }

        private static bool VerifyMapperResultHelper( this Mapper ultraMapper,
            object source, object target, ReferenceTracker referenceTracking )
        {
            //sharing the same reference or both null
            if( Object.ReferenceEquals( source, target ) )
                return true;

            //either source or target is null 
            if( source == null || target == null )
                return false;

            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            if( !sourceType.IsValueType && source != null )
            {
                if( referenceTracking.Contains( source, targetType ) )
                    return true;

                if( !sourceType.IsValueType )
                    referenceTracking.Add( source, targetType, target );
            }

            //same value type: just compare their values
            if( sourceType == targetType && sourceType.IsBuiltIn( false ) )
                return source.Equals( target );

            if( sourceType.IsEnumerable() && targetType.IsEnumerable() )
            {
                var firstPos = (source as IEnumerable).GetEnumerator();
                var secondPos = (target as IEnumerable).GetEnumerator();

                var hasFirst = firstPos.MoveNext();
                var hasSecond = secondPos.MoveNext();

                while( hasFirst && hasSecond )
                {
                    if( !VerifyMapperResultHelper( ultraMapper, firstPos.Current, secondPos.Current, referenceTracking ) )
                        return false;

                    hasFirst = firstPos.MoveNext();
                    hasSecond = secondPos.MoveNext();
                }

                if( hasFirst ^ hasSecond )
                    return false;
            }

            var typeMapping = ultraMapper.MappingConfiguration[
                source.GetType(), target.GetType() ];

            foreach( var mapping in typeMapping.MemberMappings.Values )
            {
                if( mapping.MappingResolution == Internals.MappingResolution.RESOLVED_BY_CONVENTION
                    && typeMapping.IgnoreMemberMappingResolvedByConvention == true ) continue;

                var sourceValue = mapping.SourceMember
                    .ValueGetter.Compile().DynamicInvoke( source );

                var targetValue = mapping.TargetMember
                    .ValueGetter.Compile().DynamicInvoke( target );

                var converter = mapping.CustomConverter;
                if( converter != null )
                {
                    if( mapping.MappingExpression.Parameters[ 0 ].Type == typeof( ReferenceTracker ) )
                    {
                        //sourceValue = converter.Compile().DynamicInvoke( referenceTracking, sourceValue, targetValue );
                    }
                    else
                    {
                        sourceValue = converter.Compile().DynamicInvoke( sourceValue );
                    }
                }

                if( !mapping.SourceMember.MemberType.IsValueType && sourceValue != null )
                {
                    if( referenceTracking.Contains( sourceValue, mapping.TargetMember.MemberType ) )
                        continue;

                    var result = VerifyMapperResultHelper( ultraMapper, sourceValue, targetValue, referenceTracking );
                    if( !result ) return false;

                    referenceTracking.Add( sourceValue, mapping.TargetMember.MemberType, targetValue );
                }

                if( Object.ReferenceEquals( sourceValue, targetValue ) )
                    continue;

                bool isSourcePropertyNullable = Nullable.GetUnderlyingType( mapping.SourceMember.MemberType ) != null;
                bool isTargetPropertyNullable = Nullable.GetUnderlyingType( mapping.TargetMember.MemberType ) != null;

                if( isSourcePropertyNullable && !isTargetPropertyNullable && sourceValue == null
                    && targetValue.Equals( mapping.TargetMember.MemberType.GetDefaultValueViaActivator() ) )
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
                        if( !VerifyMapperResultHelper( ultraMapper, firstPos.Current, secondPos.Current, referenceTracking ) )
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
