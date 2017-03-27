using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    /// <summary>
    /// Maps simple types and return a list of objectPair
    /// that need to be recursively mapped
    /// </summary>
    public class ReferenceMapperWithMemberMapping
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

        public virtual bool CanHandle( Type source, Type target )
        {
            bool valueTypes = source.IsValueType
                && target.IsValueType;

            bool builtInTypes = source.IsBuiltInType( false )
                && target.IsBuiltInType( false );

            return !valueTypes && !builtInTypes && !source.IsEnumerable();
        }

        protected virtual object GetMapperContext( TypeMapping typeMapping )
        {
            return new ReferenceMapperWithMemberMappingContext( typeMapping );
        }

        public LambdaExpression GetMappingExpression( TypeMapping typeMapping )
        {
            var context = GetMapperContext( typeMapping ) as ReferenceMapperWithMemberMappingContext;

            var memberMappingBody = new MemberMappingMapper().GetMemberMappings( typeMapping );

            if( memberMappingBody.NodeType == ExpressionType.Default && memberMappingBody.Type == typeof( void ) )
            {
                return typeMapping.GlobalConfiguration.Configuration[ typeMapping.TypePair ]
                    .Mapper.GetMappingExpression( typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );
            }

            var typeMappingLambdaExpression = typeMapping.Mapper.GetMappingExpression( typeMapping.TypePair.SourceType,
                typeMapping.TypePair.TargetType );

            ////per eliminare memberMappingBody...
            //if( typeMappingLambdaExpression.Body.NodeType == ExpressionType.Default && typeMappingLambdaExpression.Body.Type == typeof( void ) )
            //{
            //    return typeMappingLambdaExpression;
            //}

            Expression typeMappingExpression = typeMappingLambdaExpression.Body
                .ReplaceParameter( context.ReturnObject, context.ReturnObject.Name )
                .ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name )
                .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name );

            if( typeMappingLambdaExpression.ReturnType == typeof( List<ObjectPair> ) )
            {
                var addRange = context.ReturnObject.Type.GetMethod( nameof( List<ObjectPair>.AddRange ) );

                var objPairs = Expression.Parameter( typeof( List<ObjectPair> ), "objPairs" );
                typeMappingExpression = Expression.Block
                (
                    new[] { objPairs },

                    Expression.Assign( objPairs, typeMappingExpression ),
                    Expression.IfThen
                    (
                        Expression.NotEqual( objPairs, Expression.Constant( null, typeof( List<ObjectPair> ) ) ),
                        Expression.Call( context.ReturnObject, addRange, objPairs )
                    )
               );
            }

            var body = Expression.Block
            (
                new[] { context.ReturnObject },

                Expression.Assign( context.ReturnObject, Expression.New( context.ReturnObject.Type ) ),
                typeMappingExpression,

                context.ReturnObject
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                context.ReferenceTrack.Type, context.SourceInstance.Type,
                context.TargetInstance.Type, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }
    }

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
            var context = new ReferenceMapperWithMemberMappingContext( typeMapping );

            //since nested selectors are supported, we sort membermappings to grant
            //that we assign outer objects first
            var validMappings = typeMapping.MemberMappings.Values.ToList();
            if( typeMapping.IgnoreMemberMappingResolvedByConvention )
            {
                validMappings = validMappings.Where( mapping =>
                    mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();
            }

            var addCalls = validMappings.OrderBy( mm => mm, _memberComparer )
                .Where( mapping => !mapping.SourceMember.Ignore && !mapping.TargetMember.Ignore )
                .Select( mapping => GetMemberMapping( context, mapping ) ).ToList();

            return !addCalls.Any() ? (Expression)Expression.Empty() : Expression.Block( addCalls );
        }

        private Expression GetMemberMapping( ReferenceMapperWithMemberMappingContext context, MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.Mapper is ReferenceMapper )
                return GetComplexMemberExpression( context, mapping, memberContext );

            return GetSimpleMemberExpression( context, mapping );
        }

        private Expression GetComplexMemberExpression( ReferenceMapperWithMemberMappingContext context, MemberMapping mapping, MemberMappingContext memberContext )
        {
            /* SOURCE (NULL) -> TARGET = NULL
             * 
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
             * 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NULL) = ASSIGN NEW OBJECT 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
             */

            Expression lookupCall = Expression.Call( Expression.Constant( ReferenceMapper.refTrackingLookup.Target ),
                ReferenceMapper.refTrackingLookup.Method, context.ReferenceTrack,
                memberContext.SourceMember, Expression.Constant( memberContext.TargetMember.Type ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( ReferenceMapper.addToTracker.Target ),
                ReferenceMapper.addToTracker.Method, context.ReferenceTrack, memberContext.SourceMember,
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
                                AssignInstanceToTargetMember( memberContext, mapping ),

                                mapping.MappingExpression.Body
                                    .ReplaceParameter( memberContext.SourceMember, mapping.MappingExpression.Parameters[ 1 ].Name )
                                    .ReplaceParameter( memberContext.TargetMember, mapping.MappingExpression.Parameters[ 2 ].Name ),

                                //cache reference
                                addToLookupCall
                            )
                        )
                    )
                ),

                memberContext.TargetMemberValueSetter
            );
        }

        private Expression GetSimpleMemberExpression( ReferenceMapperWithMemberMappingContext context, MemberMapping mapping )
        {
            ParameterExpression value = Expression.Parameter( mapping.TargetMember.MemberType, "returnValue" );

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            return Expression.Block
            (
                new[] { value },

                Expression.Assign( value,
                    Expression.Invoke( mapping.MappingExpression, mapping.SourceMember.ValueGetter.Body
                        .ReplaceParameter( context.SourceInstance, mapping.SourceMember.ValueGetter.Parameters[ 0 ].Name ) ) ),

                mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( context.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( value, targetSetterMemberParamName )
            );
        }

        protected virtual Expression AssignInstanceToTargetMember( MemberMappingContext context, MemberMapping mapping )
        {
            var newInstanceExp = Expression.New( context.TargetMember.Type );

            if( mapping.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                return Expression.Assign( context.TargetMember, newInstanceExp );

            return Expression.Block
            (
                Expression.Assign( context.TargetMember, context.TargetMemberValueGetter ),

                Expression.IfThen
                (
                    Expression.Equal( context.TargetMember, context.TargetMemberNullValue ),
                    Expression.Assign( context.TargetMember, newInstanceExp )
                )
            );
        }
    }
}
