using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : IMapperExpressionBuilder, ITypeMapperExpression
    {
        public readonly MapperConfiguration MapperConfiguration;

        public ReferenceMapper( MapperConfiguration configuration )
        {
            this.MapperConfiguration = configuration;
        }

#if DEBUG
        private static void debug( object o ) => Console.WriteLine( o );

        protected static readonly Expression<Action<object>> debugExp =
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
                && target.IsValueType;

            bool builtInTypes = source.IsBuiltInType( false )
                && target.IsBuiltInType( false );

            return !valueTypes && !builtInTypes && !source.IsEnumerable();
        }

        protected LambdaExpression GetMappingExpression( ReferenceMapperContext context )
        {
            var expressionBody = this.GetExpressionBody( context );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceInstance.Type,
                context.TargetInstance.Type, context.ReturnObject.Type );

            return Expression.Lambda( delegateType, expressionBody,
                context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }

        public LambdaExpression GetMappingExpression( Type source, Type target )
        {
            var context = this.GetMapperContext( source, target ) as ReferenceMapperContext;
            return GetMappingExpression( context );
        }

        protected virtual object GetMapperContext( Type source, Type target )
        {
            return new ReferenceMapperContext( source, target );
        }

        protected virtual Expression GetExpressionBody( ReferenceMapperContext context )
        {
            return Expression.Block
            (
                new ParameterExpression[] { context.ReturnObject },

                                    this.GetInnerBody( context ),

                                    new MemberMappingMapper().GetMemberMappings(
                                        MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ] ),

                context.ReturnObject
            );
        }

        protected virtual Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;

            return Expression.Block
            (
                this.GetTargetInstanceAssignment( contextObj ),

                //assign to the object to return
                Expression.Assign( context.ReturnObject, Expression.New(
                    context.ReturnTypeConstructor, context.SourceInstance, context.TargetInstance ) )
            );
        }

        protected virtual Expression GetTargetInstanceAssignment( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;
            var newInstanceExp = Expression.New( context.TargetInstance.Type );

            var typeMapping = MapperConfiguration[ context.SourceInstance.Type,
                context.TargetInstance.Type ];

            if( typeMapping.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                return Expression.Assign( context.TargetInstance, newInstanceExp );

            return Expression.IfThen
            (
                Expression.Equal( context.TargetInstance, context.TargetNullValue ),
                Expression.Assign( context.TargetInstance, newInstanceExp )
            );
        }
    }

    /// <summary>
    /// Maps simple types and return a list of objectPair
    /// that need to be recursively mapped
    /// </summary>
    public class ReferenceMapperWithMemberMapping : IMapperExpressionBuilder, ITypeMappingMapperExpression
    {
        private static MemberMappingComparer _memberComparer = new MemberMappingComparer();
        //private static HashSet<TypePair> pairs = new HashSet<TypePair>();

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

            //LambdaExpression typeMappingExp = null;

            //avoid infinite recursion and stackoverflow
            //var pair = new TypePair( typeMapping.TypePair.SourceType,
            //    typeMapping.TypePair.TargetType );

            //if( !pairs.Contains( pair ) )
            //{
            //    pairs.Add( pair );

            //    typeMappingExp =
            //        typeMapping.GlobalConfiguration.Configuration[
            //        typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType ]
            //        .Mapper.GetMappingExpression( typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );

            //    pairs.Remove( pair );
            //}

            //Func<LambdaExpression, Expression> createAddCalls = ( lambdaExp ) =>
            //{
            //    if( lambdaExp.ReturnType == context.ReturnElementType )
            //    {
            //        var objPair = Expression.Variable( context.ReturnElementType, "objPair" );

            //        return Expression.Block
            //        (
            //            new[] { objPair },

            //            Expression.Assign( objPair, lambdaExp.Body ),

            //            Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
            //                Expression.Call( context.ReturnObject, addMethod, objPair ) )
            //        );
            //    }

            //    return lambdaExp.Body;
            //};

            var test = typeMapping.Mapper.GetMappingExpression( typeMapping.TypePair.SourceType,
                typeMapping.TypePair.TargetType );

            var memberMappingBody = new MemberMappingMapper().GetMemberMappings( typeMapping );
            //var typeMappingBody = typeMappingExp != null ? createAddCalls( typeMappingExp ) : Expression.Empty();

            if( memberMappingBody.NodeType == ExpressionType.Default && memberMappingBody.Type == typeof( void ) )
            {
                return typeMapping.GlobalConfiguration.Configuration[ typeMapping.TypePair ]
                    .Mapper.GetMappingExpression( typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );
            }

            var body = Expression.Block
            (
                new[] { context.ReturnObject },

                Expression.Assign( context.ReturnObject, Expression.New( context.ReturnObject.Type ) ),

                memberMappingBody
                    .ReplaceParameter( context.ReturnObject, context.ReturnObject.Name )
                    .ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name )
                    .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                    .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name ),

                 test.Body
                     .ReplaceParameter( context.ReturnObject, context.ReturnObject.Name )
                     .ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name )
                     .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                     .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name ),

                 //typeMappingBody
                 //    .ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name )
                 //    .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                 //    .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name ),

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

        public Expression GetMemberMapping( ReferenceMapperWithMemberMappingContext context, MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.Expression.ReturnType == typeof( List<ObjectPair> ) )
            {
                return Expression.Block
                (
                    new[] { memberContext.SourceMember, memberContext.TargetMember },

                    Expression.Assign( memberContext.SourceMember, mapping.SourceMember.ValueGetter.Body.ReplaceParameter(
                        memberContext.SourceInstance, mapping.SourceMember.ValueGetter.Parameters[ 0 ].Name ) ),

                    AssignInstanceToTargetMember( memberContext, mapping ),

                    mapping.Expression.Body
                        .ReplaceParameter( memberContext.SourceMember, mapping.Expression.Parameters[ 1 ].Name )
                        .ReplaceParameter( memberContext.TargetMember, mapping.Expression.Parameters[ 2 ].Name ),

                    memberContext.TargetMemberValueSetter
                );
            }

            else if( mapping.Expression.ReturnType == context.ReturnElementType )
            {
                var addMethod = context.ReturnObject.Type.GetMethod( nameof( List<ObjectPair>.Add ) );

                /* SOURCE (NULL) -> TARGET = NULL
                 * 
                 * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
                 * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
                 * 
                 * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NULL) = ASSIGN NEW OBJECT 
                 * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NOT NULL) = KEEP USING INSTANCE OR CREATE NEW OBJECT
                 */

                Expression lookupCall = Expression.Call( Expression.Constant( ReferenceMapper.refTrackingLookup.Target ),
                    ReferenceMapper.refTrackingLookup.Method, context.ReferenceTrack,
                    memberContext.SourceMember, Expression.Constant( memberContext.TargetMember.Type ) );

                Expression addToLookupCall = Expression.Call( Expression.Constant( ReferenceMapper.addToTracker.Target ),
                    ReferenceMapper.addToTracker.Method, context.ReferenceTrack, memberContext.SourceMember,
                    Expression.Constant( memberContext.TargetMember.Type ), memberContext.TargetMember );

                return Expression.Block
                (
                    new[] { memberContext.SourceMember, memberContext.TargetMember },

                    Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                    Expression.IfThenElse
                    (
                         Expression.Equal( memberContext.SourceMember, memberContext.SourceMemberNullValue ),

                         Expression.Assign( memberContext.TargetMember, memberContext.TargetMemberNullValue ),

                         Expression.Block
                         (
                            //object lookup
                            Expression.Assign( memberContext.TargetMember,
                                Expression.Convert( lookupCall, memberContext.TargetMember.Type ) ),

                            Expression.IfThen
                            (
                                 Expression.Equal( memberContext.TargetMember, memberContext.TargetMemberNullValue ),
                                 Expression.Block
                                 (
                                     AssignInstanceToTargetMember( memberContext, mapping ),

                                     memberContext.TargetMemberValueSetter,

                                     //cache reference
                                     addToLookupCall,

                                     Expression.Call
                                     (
                                         context.ReturnObject, addMethod,
                                         Expression.New
                                         (
                                             typeof( ObjectPair ).GetConstructors()[ 0 ],
                                             Expression.Convert( memberContext.SourceMember, typeof( object ) ),
                                             Expression.Convert( memberContext.TargetMember, typeof( object ) )
                                         )
                                     )
                                )
                            )
                        )
                    )
                );
            }

            ParameterExpression value = Expression.Parameter( mapping.TargetMember.MemberType, "returnValue" );

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            return Expression.Block
            (
                new[] { value },

                Expression.Assign( value,
                    Expression.Invoke( mapping.Expression, mapping.SourceMember.ValueGetter.Body
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

            return Expression.IfThen
            (
                Expression.Equal( context.TargetMember, context.TargetMemberNullValue ),
                Expression.Assign( context.TargetMember, newInstanceExp )
            );
        }
    }
}
