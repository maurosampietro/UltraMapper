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

            var typeMapping = context.MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];

            var memberMappings = this.GetMemberMappingsExpression( typeMapping )
                .ReplaceParameter( context.Mapper, context.Mapper.Name )
                .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name );

            var expression = Expression.Block
            (
                new[] { context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( context.MapperInstance ) ),

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
                var sourceCountMethod = CollectionMapper.GetCountMethodStatic( sourceType );
                if( sourceCountMethod != null )
                {
                    Expression sourceCountMethodCallExp;
                    if( sourceCountMethod.IsStatic )
                        sourceCountMethodCallExp = Expression.Call( null, sourceCountMethod, sourceValue );
                    else sourceCountMethodCallExp = Expression.Call( sourceValue, sourceCountMethod );

                    var ctorArgTypes = new[] { typeof( int ) };
                    var ctorInfo = targetType.GetConstructor( ctorArgTypes );

                    return Expression.New( ctorInfo, sourceCountMethodCallExp );
                }
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
                .Select( mapping => mapping.MemberMappingExpression.Body )
                .ToList();
        }

        protected Expression GetMemberMappingsExpression( TypeMapping typeMapping )
        {
            var memberMappingExps = this.GetMemberMappingExpressions( typeMapping );
            return !memberMappingExps.Any() ? Expression.Empty() :
                Expression.Block( memberMappingExps );
        }
        #endregion
    }
}
