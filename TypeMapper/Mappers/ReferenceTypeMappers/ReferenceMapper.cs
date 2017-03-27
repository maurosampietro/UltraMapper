using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : IMapperExpressionBuilder
    {
        public readonly MapperConfiguration MapperConfiguration;

        public ReferenceMapper( MapperConfiguration configuration )
        {
            this.MapperConfiguration = configuration;
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

            return !valueTypes && !builtInTypes && !source.IsEnumerable();
        }

        public LambdaExpression GetMappingExpression( Type source, Type target )
        {
            var context = this.GetMapperContext( source, target ) as ReferenceMapperContext;
            return GetMappingExpression( context );
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

        protected virtual object GetMapperContext( Type source, Type target )
        {
            return new ReferenceMapperContext( source, target );
        }

        protected virtual Expression GetExpressionBody( ReferenceMapperContext context )
        {
            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];
            var memberMappings = new MemberMappingMapper().GetMemberMappings( typeMapping )
                .ReplaceParameter( context.ReturnObject, context.ReturnObject.Name )
                .ReplaceParameter( context.ReferenceTrack, context.ReferenceTrack.Name );

            return Expression.Block
            (
                new ParameterExpression[] { context.ReturnObject },

                this.GetInnerBody( context ),
                memberMappings,

                context.ReturnObject
            );
        }

        protected virtual Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;
            return Expression.Assign( context.ReturnObject, Expression.New( context.ReturnObject.Type ) );
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
}
