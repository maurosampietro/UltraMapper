using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;
using UltraMapper.Mappers.MapperContexts;

namespace UltraMapper.Mappers
{
    public class MemberMappingMapper
    {
        private static MemberMappingComparer _memberComparer = new MemberMappingComparer();

        private class MemberMappingComparer : IComparer<MemberMapping>
        {
            public int Compare( MemberMapping x, MemberMapping y )
            {
                var xGetter = x.TargetMember.ValueGetter.ToString();
                var yGetter = y.TargetMember.ValueGetter.ToString();

                int xCount = xGetter.Split( '.' ).Count();
                int yCount = yGetter.Split( '.' ).Count();

                if( xCount > yCount ) return 1;
                if( xCount < yCount ) return -1;

                return 0;
            }
        }

        public Expression GetMemberMappings( TypeMapping typeMapping )
        {
            var context = new ReferenceMapperContext( typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );

            //since nested selectors are supported, we sort membermappings to grant
            //that we assign outer objects first
            var memberMappings = typeMapping.MemberMappings.Values.ToList();
            if( typeMapping.IgnoreMemberMappingResolvedByConvention )
            {
                memberMappings = memberMappings.Where( mapping =>
                    mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();
            }

            var memberMappingExps = memberMappings.OrderBy( mm => mm, _memberComparer )
                .Where( mapping => !mapping.SourceMember.Ignore && !mapping.TargetMember.Ignore )
                .Select( mapping => GetMemberMapping( mapping ) ).ToList();

            return !memberMappingExps.Any() ? (Expression)Expression.Empty() : Expression.Block( memberMappingExps );
        }

        private Expression GetMemberMapping( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.Mapper is ReferenceMapper )
                return GetComplexMemberExpression( mapping, memberContext );

            return GetSimpleMemberExpression( mapping, memberContext );
        }

        private Expression GetComplexMemberExpression( MemberMapping mapping, MemberMappingContext memberContext )
        {
            /* SOURCE (NULL) -> TARGET = NULL
             * 
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
             * 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NULL) = ASSIGN NEW OBJECT 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
             */

            var recursor = Expression.Variable( CollectionMapper._runtimeRecursor.GetType(), "recursor" );
            var mapMethod = CollectionMapper.getTypeMapperMapGenericMethod().MakeGenericMethod( memberContext.SourceMember.Type, memberContext.TargetMember.Type );

            Expression lookupCall = Expression.Call( Expression.Constant( ReferenceMapper.refTrackingLookup.Target ),
                ReferenceMapper.refTrackingLookup.Method, memberContext.ReferenceTracker,
                memberContext.SourceMember, Expression.Constant( memberContext.TargetMember.Type ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( ReferenceMapper.addToTracker.Target ),
                ReferenceMapper.addToTracker.Method, memberContext.ReferenceTracker, memberContext.SourceMember,
                Expression.Constant( memberContext.TargetMember.Type ), memberContext.TargetMember );

            return Expression.Block
            (
                new[] { memberContext.TrackedReference, memberContext.SourceMember, memberContext.TargetMember },

                Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                Expression.IfThenElse
                (
                     Expression.Equal( memberContext.SourceMember, memberContext.SourceMemberNullValue ),

                     Expression.Assign( memberContext.TargetMember, memberContext.TargetMemberNullValue ),

                     Expression.Block
                     (
                        //object lookup. An intermediate variable (TrackedReference) is needed in order to deal with ReferenceMappingStrategies
                        Expression.Assign( memberContext.TrackedReference,
                            Expression.Convert( lookupCall, memberContext.TargetMember.Type ) ),

                        Expression.IfThenElse
                        (
                            Expression.NotEqual( memberContext.TrackedReference, memberContext.TargetMemberNullValue ),
                            Expression.Assign( memberContext.TargetMember, memberContext.TrackedReference ),
                            Expression.Block
                            (
                                new[] { recursor },

                                ((IReferenceMapperExpressionBuilder)mapping.Mapper)
                                    .GetTargetInstanceAssignment( memberContext, mapping ),

                                Expression.Assign( recursor, Expression.Constant( CollectionMapper._runtimeRecursor ) ),
                                Expression.Call( recursor, mapMethod, memberContext.SourceMember,
                                    memberContext.TargetMember, memberContext.ReferenceTracker ),

                                ////Add to the list of instance-pair to recurse on (only way to avoid StackOverflow if the mapping object contains anywhere 
                                ////down the tree a member of the same type of the mapping object itself)
                                //Expression.Call
                                //(
                                //    memberContext.ReturnObject, memberContext.AddObjectPairToReturnList,
                                //    Expression.New( memberContext.ReturnElementConstructor,
                                //        memberContext.SourceMember, memberContext.TargetMember )
                                //),

                                //cache reference
                                addToLookupCall
                            )
                        )
                    )
                ),

                memberContext.TargetMemberValueSetter
            );
        }

        private Expression GetSimpleMemberExpression( MemberMapping mapping, MemberMappingContext memberContext )
        {
            ParameterExpression value = Expression.Parameter( mapping.TargetMember.MemberType, "returnValue" );

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            return Expression.Block
            (
                new[] { memberContext.SourceMember, value },

                Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                Expression.Assign( value, mapping.MappingExpression.Body
                    .ReplaceParameter( memberContext.SourceMember,
                        mapping.MappingExpression.Parameters[ 0 ].Name ) ),

                mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( value, targetSetterMemberParamName )
            );
        }
    }
}
