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

        public static Func<ReferenceTracker, object, Type, object> refTrackingLookup =
         ( referenceTracker, sourceInstance, targetType ) =>
         {
             referenceTracker.TryGetValue( sourceInstance, targetType, out object targetInstance );
             return targetInstance;
         };

        public static Action<ReferenceTracker, object, Type, object> addToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance, targetType, targetInstance );
        };

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

            //If we are just cloning (ie: mapping on the same type) we prefer to use exactly the 
            //same runtime-type used in the source (in order to manage abstract classes, interfaces and inheritance). 
            if( targetType.IsAssignableFrom( sourceType ) )
            {
                MethodInfo getTypeMethodInfo = typeof( object ).GetMethod( nameof( object.GetType ) );
                var getSourceType = Expression.Call( sourceValue, getTypeMethodInfo );

                return Expression.Convert( Expression.Call( null, typeof( InstanceFactory ).GetMethods()[ 1 ],
                    getSourceType, Expression.Constant( null, typeof( object[] ) ) ), targetType );
            }

            var defaultCtor = targetType.GetConstructor( Type.EmptyTypes );
            if( defaultCtor != null )
                return Expression.New( targetType );

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
            var memberMappings = typeMapping.MemberMappings.Values.ToList();
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

            return !memberMappingExps.Any() ? (Expression)Expression.Empty() : Expression.Block( memberMappingExps );
        }

        protected Expression GetComplexMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.CustomConverter != null )
            {
                var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
                var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

                var valueReaderExp = Expression.Invoke( mapping.CustomConverter, memberContext.SourceMemberValueGetter );

                return mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( valueReaderExp, targetSetterValueParamName )
                    .ReplaceParameter( valueReaderExp, mapping.CustomConverter.Parameters[ 0 ].Name );
            }

            var memberAssignmentExp = ((IMemberMappingExpression)mapping.Mapper)
                .GetMemberAssignment( memberContext );

            var referenceTrackingExp = ReferenceTrackingExpression.GetMappingExpression(
                memberContext.ReferenceTracker, memberContext.SourceMember,
                memberContext.TargetMember, memberAssignmentExp,
                memberContext.Mapper, _mapper,
                Expression.Constant( mapping ) );

            return Expression.Block
            (
                new[] { memberContext.TrackedReference, memberContext.SourceMember, memberContext.TargetMember },

                Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                referenceTrackingExp,            

                memberContext.TargetMemberValueSetter
            );
        }

        protected Expression GetSimpleMemberExpressionInternal(
            LambdaExpression mappingExpression, ParameterExpression targetInstance,
            Expression sourceMemberValueGetter, LambdaExpression targetMemberValueSetter )
        {
            var targetSetterInstanceParamName = targetMemberValueSetter.Parameters[ 0 ].Name;
            var targetSetterValueParamName = targetMemberValueSetter.Parameters[ 1 ].Name;

            var valueReaderExp = mappingExpression.Body.ReplaceParameter(
                sourceMemberValueGetter, mappingExpression.Parameters[ 0 ].Name );

            if( mappingExpression.Parameters[ 0 ].Type == typeof( ReferenceTracker ) )
            {
                valueReaderExp = mappingExpression.Body.ReplaceParameter(
                    sourceMemberValueGetter, mappingExpression.Parameters[ 1 ].Name );
            }

            return targetMemberValueSetter.Body
                .ReplaceParameter( targetInstance, targetSetterInstanceParamName )
                .ReplaceParameter( valueReaderExp, targetSetterValueParamName );
        }

        protected Expression GetSimpleMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            return GetSimpleMemberExpressionInternal( mapping.MappingExpression,
                memberContext.TargetInstance, memberContext.SourceMemberValueGetter,
                mapping.TargetMember.ValueSetter );
        }

        #endregion
    }
}
