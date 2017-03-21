using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : IMapperExpressionBuilder, IMemberMappingMapperExpression//, ITypeMappingMapperExpression
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

        protected static Func<ReferenceTracking, object, Type, object> refTrackingLookup =
         ( referenceTracker, sourceInstance, targetType ) =>
         {
             object targetInstance;
             referenceTracker.TryGetValue( sourceInstance, targetType, out targetInstance );

             return targetInstance;
         };

        protected static Action<ReferenceTracking, object, Type, object> addToTracker =
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

        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            var context = this.GetMapperContext( mapping ) as ReferenceMapperContext;
            return GetMappingExpression( context );
        }

        protected virtual object GetMapperContext( MemberMapping mapping )
        {
            return new ReferenceMapperContext( mapping );
        }

        protected virtual Expression GetExpressionBody( ReferenceMapperContext context )
        {
            /* SOURCE (NULL) -> TARGET = NULL
            * 
            * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
            * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
            * 
            * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NULL) = ASSIGN NEW OBJECT 
            * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NOT NULL) = KEEP USING INSTANCE OR CREATE NEW OBJECT
            */

            Expression lookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, context.ReferenceTrack,
                context.SourceMember, Expression.Constant( context.TargetMember.Type ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, context.ReferenceTrack, context.SourceMember,
                Expression.Constant( context.TargetMember.Type ), context.TargetMember );

            return Expression.Block
            (
                new ParameterExpression[] { context.SourceMember, context.TargetMember, context.ReturnObject },

                //read source value
                Expression.Assign( context.SourceMember, context.SourceMemberValue ),

                Expression.IfThenElse
                (
                     Expression.Equal( context.SourceMember, context.SourceMemberNullValue ),

                     Expression.Assign( context.TargetMember, context.TargetMemberNullValue ),

                     Expression.Block
                     (
                        //object lookup
                        context.TargetMemberValueSetter == null ? Expression.Default( typeof( void ) ) :
                        (Expression)Expression.Assign( context.TargetMember,
                            Expression.Convert( lookupCall, context.TargetMember.Type ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetMember, context.TargetMemberNullValue ),
                            Expression.Block
                            (
                                this.GetInnerBody( context ),

                                //cache reference
                                context.TargetMemberValueSetter == null ? Expression.Default( typeof( void ) ) : addToLookupCall
                            )
                        )
                    )
                ),

                context.TargetMemberValueSetter == null ? Expression.Default( typeof( void ) ) : context.TargetMemberValueSetter,
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
                    context.ReturnTypeConstructor, context.SourceMember, context.TargetMember ) )
            );
        }

        protected virtual Expression GetTargetInstanceAssignment( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;
            var newInstanceExp = Expression.New( context.TargetMember.Type );

            var typeMapping = MapperConfiguration[ context.SourceMember.Type,
                context.TargetMember.Type ];

            if( typeMapping.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                return Expression.Assign( context.TargetMember, newInstanceExp );

            return Expression.IfThenElse
            (
                Expression.Equal( context.TargetMemberValue, context.TargetMemberNullValue ),
                Expression.Assign( context.TargetMember, newInstanceExp ),
                Expression.Assign( context.TargetMember, context.TargetMemberValue )
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

            LambdaExpression typeMappingExp = typeMapping.Mapper?.GetMappingExpression(
                typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );

            var addMethod = context.ReturnObject.Type.GetMethod( nameof( List<ObjectPair>.Add ) );
            var addRangeMethod = context.ReturnObject.Type.GetMethod( nameof( List<ObjectPair>.AddRange ) );

            Func<MemberMapping, Expression> getMemberMapping = ( mapping ) =>
            {
                ParameterExpression value = Expression.Parameter( mapping.TargetMember.MemberType, "returnValue" );

                var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
                var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

                LambdaExpression expression = mapping.Expression;

                if( mapping.Expression.ReturnType == typeof( List<ObjectPair> ) )
                {
                    return Expression.Call( context.ReturnObject, addRangeMethod, mapping.Expression.Body );
                }

                else if( expression.ReturnType == context.ReturnElementType )
                {
                    var objPair = Expression.Variable( context.ReturnElementType, "objPair" );

                    //var targetMemberInfo = typeof( ObjectPair ).GetMember( nameof( ObjectPair.Target ) )[ 0 ];
                    //var targetAccess = Expression.MakeMemberAccess( objPair, targetMemberInfo );

                    return Expression.Block
                    (
                        new[] { value, objPair },

                        Expression.Assign( objPair, expression.Body ),

                        //Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
                        //Expression.Assign( value, Expression.Convert( targetAccess, mapping.TargetMember.MemberType ) )),

                        //mapping.TargetMember.ValueSetter.Body
                        //    .ReplaceParameter( targetInstance, targetSetterInstanceParamName )
                        //    .ReplaceParameter( value, targetSetterMemberParamName ),

                        Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null, context.ReturnElementType ) ),
                            Expression.Call( context.ReturnObject, addMethod, objPair ) )
                    );
                }


                return Expression.Block
                (
                    new[] { value },

                    Expression.Assign( value, Expression.Invoke( expression, Expression.Invoke( mapping.SourceMember.ValueGetter, context.SourceInstance ) ) ),

                    mapping.TargetMember.ValueSetter.Body
                        .ReplaceParameter( context.TargetInstance, targetSetterInstanceParamName )
                        .ReplaceParameter( value, targetSetterMemberParamName )
                );
            };

            Func<LambdaExpression, Expression> createAddCalls = ( lambdaExp ) =>
            {
                if( lambdaExp.ReturnType == context.ReturnElementType )
                {
                    var objPair = Expression.Variable( context.ReturnElementType, "objPair" );

                    return Expression.Block
                    (
                        new[] { objPair },

                        Expression.Assign( objPair, lambdaExp.Body ),

                        Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
                            Expression.Call( context.ReturnObject, addMethod, objPair ) )
                    );
                }

                return lambdaExp.Body;
            };

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
                .Select( mapping => getMemberMapping( mapping ) ).ToList();

            if( !addCalls.Any() && typeMappingExp != null )
                return typeMappingExp;

            var bodyExp = !addCalls.Any() ? (Expression)Expression.Empty() : Expression.Block( addCalls );
            var typeMappingBodyExp = typeMappingExp != null ? createAddCalls( typeMappingExp ) : Expression.Empty();

            var body = Expression.Block
            (
                new[] { context.ReturnObject },

                Expression.Assign( context.ReturnObject, Expression.New( context.ReturnObject.Type ) ),

                typeMappingBodyExp.ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name )
                    .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                    .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name ),

                bodyExp.ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name )
                    .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                    .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name ),

                 context.ReturnObject
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                context.ReferenceTrack.Type, context.SourceInstance.Type, 
                context.TargetInstance.Type, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }
    }
}
