using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders.MapperContexts;
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
        private static void _debug( object o ) => Console.WriteLine( o );

        public static readonly Expression<Action<object>> debugExp =
            ( o ) => _debug( o );
#endif

        public virtual bool CanHandle( Type source, Type target )
        {
            bool builtInTypes = source.IsBuiltIn( false )
                && target.IsBuiltIn( false );

            return !target.IsValueType && !builtInTypes;
        }

        protected virtual ReferenceMapperContext GetMapperContext( Type source, Type target, IMappingOptions options )
        {
            return new ReferenceMapperContext( source, target, options );
        }

        public virtual LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = this.GetMapperContext( source, target, options );

            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];
            var memberMappings = this.GetMemberMappings( typeMapping )
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

        public virtual Expression GetMemberAssignment( MemberMappingContext context )
        {
            Expression newInstance = this.GetMemberNewInstance( context );

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

        public virtual Expression GetMemberNewInstance( MemberMappingContext context )
        {
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

        protected Expression GetMemberMappings( TypeMapping typeMapping )
        {
            //since nested selectors are supported, we sort membermappings to grant
            //that we assign outer objects first
            var memberMappings = typeMapping.MemberToMemberMappings.Values.ToList();
            if( typeMapping.IgnoreMemberMappingResolvedByConvention == true )
            {
                memberMappings = memberMappings.Where( mapping =>
                    mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();
            }

            var memberMappingExps = memberMappings
                .Where( mapping => !mapping.Ignore )
                .Where( mapping => !mapping.SourceMember.Ignore )
                .Where( mapping => !mapping.TargetMember.Ignore )
                .OrderBy( mapping => mapping, _memberComparer )
                .Select( mapping =>
                {
                    if( mapping.Mapper is ReferenceMapper )
                        return GetComplexMemberExpression( mapping );

                    return GetSimpleMemberExpression( mapping );
                } ).ToList();

            return !memberMappingExps.Any() ?
                (Expression)Expression.Empty() :
                Expression.Block( memberMappingExps );
        }

        protected virtual Expression GetComplexMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.CustomConverter != null )
            {
                var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
                var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

                var valueReaderExp = Expression.Invoke( mapping.CustomConverter, memberContext.SourceMemberValueGetter );

                return mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( valueReaderExp, targetSetterValueParamName );
            }

            var memberAssignmentExp = ((IMemberMappingExpression)mapping.Mapper)
                .GetMemberAssignment( memberContext );

            var parameters = new List<ParameterExpression>()
            {
                memberContext.SourceMember
            };

            if( memberContext.Options.IsReferenceTrackingEnabled )
            {
                parameters.Add( memberContext.TargetMember );
                parameters.Add( memberContext.TrackedReference );

                return Expression.Block
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
            }
            else
            {
                var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                    .MakeGenericMethod( memberContext.SourceMember.Type, memberContext.TargetMember.Type );

                return Expression.Block
                (
                    parameters,

                    Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),
                    memberAssignmentExp.ReplaceParameter( memberContext.TargetMemberValueGetter, "targetValue" ),

                    Expression.Call( memberContext.Mapper, mapMethod, memberContext.SourceMember,
                        memberContext.TargetMemberValueGetter,
                        memberContext.ReferenceTracker, Expression.Constant( mapping ) )
                );
            }
        }

        private static readonly Expression<Func<string, string, object, string>> _getErrorExp =
            ( error, mapping, sourceMemberValue ) => String.Format( error, mapping, sourceMemberValue ?? "null" );

        private const string errorMsg = "Error mapping '{0}'. Value '{1}' cannot be assigned to the target.";

        protected Expression GetSimpleMemberExpression( MemberMapping mapping )
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
                Expression.Convert( memberContext.SourceMemberValueGetter, typeof( object ) )
            );

            return Expression.TryCatch
            (
                Expression.Block( typeof( void ), expression ),

                Expression.Catch( exceptionParam, Expression.Throw
                (
                    Expression.New( ctor, getErrorMsg, exceptionParam ),
                    typeof( void )
                ) )
            );
        }
        #endregion
    }
}
