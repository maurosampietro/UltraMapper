using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.Mappers.MapperContexts;

namespace UltraMapper.Mappers
{
    public class ReferenceMapper : IMemberMappingExpressionBuilder
    {
        protected readonly UltraMapper _mapper;
        public readonly Configuration MapperConfiguration;

        public ReferenceMapper( Configuration configuration )
        {
            this.MapperConfiguration = configuration;
            _mapper = new UltraMapper( configuration );
        }

#if DEBUG
        private static void debug( object o ) => Console.WriteLine( o );

        public static readonly Expression<Action<object>> debugExp =
            ( o ) => debug( o );
#endif

        public static Func<ReferenceTracking, object, Type, object> refTrackingLookup =
         ( referenceTracker, sourceInstance, targetType ) =>
         {
             object targetInstance;
             referenceTracker.TryGetValue( sourceInstance, targetType, out targetInstance );

             return targetInstance;
         };

        public static Action<ReferenceTracking, object, Type, object> addToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance, targetType, targetInstance );
        };

        public virtual bool CanHandle( Type source, Type target )
        {
            bool valueTypes = source.IsValueType
                || target.IsValueType;

            bool builtInTypes = source.IsBuiltInType( false )
                && target.IsBuiltInType( false );

            return !valueTypes && !builtInTypes;
        }

        public LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = this.GetMapperContext( source, target, options ) as ReferenceMapperContext;

            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];
            var memberMappings = this.GetMemberMappings( typeMapping )
                .ReplaceParameter( context.ReturnObject, context.ReturnObject.Name )
                .ReplaceParameter( context.Mapper, context.Mapper.Name )
                .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name );

            var expression = Expression.Block
            (
                new[] { context.ReturnObject, context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),
                this.ReturnListInitialization( context ),

                memberMappings,
                this.GetExpressionBody( context ),

                context.ReturnObject
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type, context.ReturnObject.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

        protected virtual ReferenceMapperContext GetMapperContext( Type source, Type target, IMappingOptions options )
        {
            return new ReferenceMapperContext( source, target, options );
        }

        protected virtual Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            return Expression.Empty();
        }

        protected virtual Expression ReturnListInitialization( ReferenceMapperContext contextObj )
        {
            var context = contextObj as ReferenceMapperContext;
            return Expression.Assign( context.ReturnObject, Expression.New( context.ReturnObject.Type ) );
        }

        public virtual Expression GetTargetInstanceAssignment( MemberMappingContext context, MemberMapping mapping )
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

        #region MemberMapping
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

        private Expression GetMemberMappings( TypeMapping typeMapping )
        {
            var context = new ReferenceMapperContext( typeMapping.TypePair.SourceType,
                typeMapping.TypePair.TargetType, typeMapping );

            //since nested selectors are supported, we sort membermappings to grant
            //that we assign outer objects first
            var memberMappings = typeMapping.MemberMappings.Values.ToList();
            if( typeMapping.IgnoreMemberMappingResolvedByConvention )
            {
                memberMappings = memberMappings.Where( mapping =>
                    mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();
            }

            var memberMappingExps = memberMappings
                .Where( mapping => !mapping.Ignore )
                .Where( mapping => !mapping.SourceMember.Ignore )
                .Where( mapping => !mapping.TargetMember.Ignore )
                .OrderBy( mm => mm, _memberComparer )
                .Select( mapping =>
                {
                    if( mapping.Mapper is ReferenceMapper )
                        return GetComplexMemberExpression( mapping );

                    return GetSimpleMemberExpression( mapping );
                } ).ToList();

            return !memberMappingExps.Any() ? (Expression)Expression.Empty() : Expression.Block( memberMappingExps );
        }

        private Expression GetComplexMemberExpression( MemberMapping mapping )
        {
            /* SOURCE (NULL) -> TARGET = NULL
             * 
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
             * 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NULL) = ASSIGN NEW OBJECT 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
             */
            var memberContext = new MemberMappingContext( mapping );

            var mapMethod = memberContext.RecursiveMapMethodInfo.MakeGenericMethod(
                memberContext.SourceMember.Type, memberContext.TargetMember.Type );

            Expression itemLookupCall = Expression.Call
            (
                Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method,
                memberContext.ReferenceTracker,
                memberContext.SourceMember,
                Expression.Constant( memberContext.TargetMember.Type )
            );

            Expression itemCacheCall = Expression.Call
            (
                Expression.Constant( addToTracker.Target ),
                addToTracker.Method,
                memberContext.ReferenceTracker,
                memberContext.SourceMember,
                Expression.Constant( memberContext.TargetMember.Type ),
                memberContext.TargetMember
            );

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
                            Expression.Convert( itemLookupCall, memberContext.TargetMember.Type ) ),

                        Expression.IfThenElse
                        (
                            Expression.NotEqual( memberContext.TrackedReference, memberContext.TargetMemberNullValue ),
                            Expression.Assign( memberContext.TargetMember, memberContext.TrackedReference ),
                            Expression.Block
                            (
                                ((IMemberMappingExpressionBuilder)mapping.Mapper)
                                    .GetTargetInstanceAssignment( memberContext, mapping ),

                                Expression.Call( memberContext.Mapper, mapMethod, memberContext.SourceMember,
                                    memberContext.TargetMember, memberContext.ReferenceTracker, Expression.Constant( mapping ) ),

                                //cache reference
                                itemCacheCall
                            )
                        )
                    )
                ),

                memberContext.TargetMemberValueSetter
            );
        }

        private Expression GetSimpleMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

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
        #endregion
    }
}
