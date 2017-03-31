using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.Mappers.MapperContexts;

namespace UltraMapper.Mappers
{
    public class ReferenceMapper : IReferenceMapperExpressionBuilder
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

            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];
            var memberMappings = new MemberMappingMapper().GetMemberMappings( typeMapping )
                .ReplaceParameter( context.ReturnObject, context.ReturnObject.Name )
                .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name );

            var expression = Expression.Block
            (
                new[] { context.ReturnObject },

                this.ReturnListInitialization( context ),

                this.GetExpressionBody( context ),                
                memberMappings,

                context.ReturnObject
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type, context.ReturnObject.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

        protected virtual ReferenceMapperContext GetMapperContext( Type source, Type target )
        {
            return new ReferenceMapperContext( source, target );
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
    }
}
