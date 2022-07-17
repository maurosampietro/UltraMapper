using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.ReferenceTracking;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ReferenceMapper : IMappingExpressionBuilder, IMemberMappingExpression
    {
        protected readonly Mapper _mapper;
        public readonly Configuration MapperConfiguration;

        public ReferenceMapper( Configuration configuration )
        {
            this.MapperConfiguration = configuration;
            _mapper = new Mapper( configuration );
        }

#if DEBUG
        private static void Debug( object o ) => Console.WriteLine( o );
        public static readonly Expression<Action<object>> _debugExp = o => Debug( o );
#endif

        public virtual bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            bool builtInTypes = source.EntryType.IsBuiltIn( false )
                && target.EntryType.IsBuiltIn( false );

            return !target.EntryType.IsValueType && !builtInTypes;
        }

        protected virtual ReferenceMapperContext GetMapperContext( Mapping mapping )
        { 
            return new ReferenceMapperContext( mapping );
        }

        public virtual LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var context = this.GetMapperContext( mapping );

            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];

            var memberMappings = this.GetMemberMappingsExpression( typeMapping )
                .ReplaceParameter( context.Mapper, context.Mapper.Name )
                .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name );

            var expression = Expression.Block
            (
                new[] { context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),

                memberMappings,
                this.GetExpressionBody( context )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

        protected virtual Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            return Expression.Empty();
        }

        public virtual Expression GetMemberAssignment( MemberMappingContext context, out bool needsTrackingOrRecursion )
        {
            Expression newInstance = this.GetMemberNewInstance( context, out bool isMapComplete );
            needsTrackingOrRecursion = !isMapComplete;

            if( isMapComplete )
                return newInstance;

            bool isCreateNewInstance = context.Options.ReferenceBehavior ==
                ReferenceBehaviors.CREATE_NEW_INSTANCE;

            if( isCreateNewInstance || context.TargetMemberValueGetter == null )
                return Expression.Assign( context.TargetMember, newInstance );

            //ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL
            return Expression.Block
            (
                Expression.Assign( context.TargetMember, context.TargetMemberValueGetter ),

                Expression.IfThen
                (
                    Expression.Equal( context.TargetMember, context.TargetMemberNullValue ),
                    Expression.Assign( context.TargetMember, newInstance )
                )
            );
        }

        public virtual Expression GetMemberNewInstance( MemberMappingContext context, out bool isMapComplete )
        {
            isMapComplete = false;
            return GetMemberNewInstanceInternal( context.SourceMemberValueGetter,
                context.SourceMember.Type, context.TargetMember.Type, context.Options );
        }

        protected virtual Expression GetMemberNewInstanceInternal( Expression sourceValue,
            Type sourceType, Type targetType, IMappingOptions options )
        {
            if( options?.CustomTargetConstructor != null )
                return Expression.Invoke( options.CustomTargetConstructor );

            if( targetType.IsArray )
            {
                var sourceCountMethod = CollectionMapper.GetCountMethod( sourceType );

                Expression sourceCountMethodCallExp;
                if( sourceCountMethod.IsStatic )
                    sourceCountMethodCallExp = Expression.Call( null, sourceCountMethod, sourceValue );
                else sourceCountMethodCallExp = Expression.Call( sourceValue, sourceCountMethod );

                return Expression.NewArrayInit( targetType, sourceCountMethodCallExp );
            }

            var defaultCtor = targetType.GetConstructor( Type.EmptyTypes );
            if( defaultCtor != null )
                return Expression.New( targetType );

            //DON'T TRY TO AUTOMATICALLY HANDLE INHERITANCE WITH CONCRETE OBJECTS HERE
            //JUST CONFIGURE THAT OUT: cfg.MapTypes<CA, CB>( () => new ConcreteType() )

            //If we are just cloning (ie: mapping on the same type) we prefer to use exactly the 
            //same runtime-type used in the source (in order to manage abstract classes, interfaces and inheritance). 
            if( targetType.IsAssignableFrom( sourceType ) && (targetType.IsAbstract || targetType.IsInterface) )
            {
                MethodInfo getTypeMethodInfo = typeof( object ).GetMethod( nameof( object.GetType ) );
                var getSourceType = Expression.Call( sourceValue, getTypeMethodInfo );

                var createObjectMethod = typeof( InstanceFactory ).GetMethods()
                    .Where( m => m.Name == nameof( InstanceFactory.CreateObject ) )
                    .Where( m => !m.IsGenericMethod )
                    .First();

                return Expression.Convert( Expression.Call( null, createObjectMethod, getSourceType ), targetType );

                //static should be faster but cannot handle non-concrete objects the same way
                //return Expression.Convert( Expression.Call( null, typeof( InstanceFactory ).GetMethods()[ 1 ],
                //   Expression.Constant( sourceType ), Expression.Constant( null, typeof( object[] ) ) ), targetType );
            }

            if( targetType.IsInterface )
            {
                throw new Exception( $"Type {targetType} is an interface but what type to use cannot be inferred. " +
                    $"Please configure what type to use like this: cfg.MapTypes<IA, IB>( () => new ConcreteTypeBImplementingIB() ) " );
            }

            throw new Exception( $"Type {targetType} does not have a default constructor. " +
                $"Please provide a way to construct the type like this: cfg.MapTypes<A, B>( () => new B(param1,param2,...) ) " );
        }

        #region MemberMapping
        private static readonly Expression<Func<string, string, object, Type, Type, string>> _getErrorExp =
            ( error, mapping, sourceMemberValue, sourceType, targetType ) => String.Format( error, mapping,
                sourceMemberValue ?? "null", sourceType.GetPrettifiedName(), targetType.GetPrettifiedName() );

        private const string errorMsg = "Error mapping '{0}'. Value '{1}' (of type '{2}') cannot be assigned to the target (of type '{3}').";

        private static readonly MemberMappingComparer _memberComparer = new MemberMappingComparer();

        private class MemberMappingComparer : IComparer<MemberMapping>
        {
            public int Compare( MemberMapping x, MemberMapping y )
            {
                int xCount = x.TargetMember.MemberAccessPath.Count;
                int yCount = y.TargetMember.MemberAccessPath.Count;

                if( xCount > yCount ) return 1;
                if( xCount < yCount ) return -1;

                return 0;
            }
        }

        private List<Expression> GetMemberMappingExpressions( TypeMapping typeMapping )
        {
            //since nested selectors are supported, we sort membermappings to grant
            //that we assign outer objects first
            var memberMappings = typeMapping.MemberToMemberMappings.Values.ToList();
            if( typeMapping.IgnoreMemberMappingResolvedByConvention == true )
            {
                memberMappings = memberMappings.Where( mapping =>
                    mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();
            }

            return memberMappings
                .Where( mapping => !mapping.Ignore )
                .Where( mapping => !mapping.SourceMember.Ignore )
                .Where( mapping => !mapping.TargetMember.Ignore )
                .OrderBy( mapping => mapping, _memberComparer )
                .Select( mapping => GetMemberMappingExpression( mapping ).Body )
                .ToList();
        }

        public LambdaExpression GetMemberMappingExpression( MemberMapping memberMapping )
        {
            if( memberMapping.Mapper is ReferenceMapper )
                return GetComplexMemberExpression( memberMapping );

            return GetSimpleMemberExpression( memberMapping );
        }

        protected Expression GetMemberMappingsExpression( TypeMapping typeMapping )
        {
            var memberMappingExps = this.GetMemberMappingExpressions( typeMapping );
            return !memberMappingExps.Any() ? Expression.Empty() :
                Expression.Block( memberMappingExps );
        }

        private LambdaExpression GetComplexMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.CustomConverter != null )
            {
                var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
                var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

                var valueReaderExp = Expression.Invoke( mapping.CustomConverter, memberContext.SourceMemberValueGetter );

                var exp = mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( valueReaderExp, targetSetterValueParamName );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }


            var memberAssignmentExp = ((IMemberMappingExpression)mapping.Mapper)
                .GetMemberAssignment( memberContext, out bool needsTrackingOrRecursion );

            if( !needsTrackingOrRecursion )
            {
                var exp = memberAssignmentExp
                    .ReplaceParameter( memberContext.SourceMemberValueGetter, "sourceValue" );

                //if a setter method was provided or resolved a target value getter may be missing
                if( memberContext.TargetMemberValueGetter != null )
                    exp = exp.ReplaceParameter( memberContext.TargetMemberValueGetter, "targetValue" );
                else // if( memberContext.TargetMemberValueSetter != null ) fails directly if not resolved/provided
                    exp = exp.ReplaceParameter( memberContext.TargetMemberValueSetter, "targetValue" );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }

            if( memberContext.Options.IsReferenceTrackingEnabled )
            {
                var parameters = new List<ParameterExpression>()
                    {
                        memberContext.SourceMember,
                        memberContext.TargetMember,
                        memberContext.TrackedReference
                    };

                var exp = Expression.Block
                (
                    parameters,

                    Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                    ReferenceTrackingExpression.GetMappingExpression
                    (
                        memberContext.ReferenceTracker, memberContext.SourceMember,
                        memberContext.TargetMember, memberAssignmentExp,
                        memberContext.Mapper, _mapper,
                        Expression.Constant( mapping )
                    ),

                    memberContext.TargetMemberValueSetter
                );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }

            else
            {
                var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                    .MakeGenericMethod( memberContext.SourceMember.Type, memberContext.TargetMember.Type );

                var exp = Expression.Block
                (
                    memberAssignmentExp
                        .ReplaceParameter( memberContext.SourceMemberValueGetter, "sourceValue" )
                        .ReplaceParameter( memberContext.TargetMemberValueGetter, "targetValue" ),

                    Expression.Call( memberContext.Mapper, mapMethod, memberContext.SourceMemberValueGetter,
                        memberContext.TargetMemberValueGetter,
                        memberContext.ReferenceTracker, Expression.Constant( mapping ) )
                );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }
        }

        private LambdaExpression GetSimpleMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            var valueReaderExp = mapping.MappingExpression.Body.ReplaceParameter(
                memberContext.SourceMemberValueGetter, mapping.MappingExpression.Parameters[ 0 ].Name );

            if( mapping.MappingExpression.Parameters[ 0 ].Type == typeof( ReferenceTracker ) )
            {
                valueReaderExp = mapping.MappingExpression.Body.ReplaceParameter(
                    memberContext.SourceMemberValueGetter, mapping.MappingExpression.Parameters[ 1 ].Name );
            }

            var expression = mapping.TargetMember.ValueSetter.Body
                .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                .ReplaceParameter( valueReaderExp, targetSetterValueParamName );

            var exceptionParam = Expression.Parameter( typeof( Exception ), "exception" );
            var ctor = typeof( ArgumentException )
                .GetConstructor( new Type[] { typeof( string ), typeof( Exception ) } );

            var getErrorMsg = Expression.Invoke
            (
                _getErrorExp,
                Expression.Constant( errorMsg ),
                Expression.Constant( memberContext.Options.ToString() ),
                Expression.Convert( memberContext.SourceMemberValueGetter, typeof( object ) ),
                Expression.Constant( memberContext.SourceMember.Type ),
                Expression.Constant( memberContext.TargetMember.Type )
            );

            expression = Expression.TryCatch
            (
                Expression.Block( typeof( void ), expression ),

                Expression.Catch( exceptionParam, Expression.Throw
                (
                    Expression.New( ctor, getErrorMsg, exceptionParam ),
                    typeof( void )
                ) )
            );

            var delegateType = typeof( Action<,> ).MakeGenericType(
            memberContext.SourceInstance.Type, memberContext.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                memberContext.SourceInstance, memberContext.TargetInstance );
        }

        private LambdaExpression ToActionWithReferenceTrackerLambda( Expression expression, MemberMappingContext memberContext )
        {
            var delegateType = typeof( Action<,,> ).MakeGenericType(
                memberContext.ReferenceTracker.Type, memberContext.SourceInstance.Type, memberContext.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
               memberContext.ReferenceTracker, memberContext.SourceInstance, memberContext.TargetInstance );
        }
        #endregion
    }
}
