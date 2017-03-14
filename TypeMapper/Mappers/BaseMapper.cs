using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class BaseMapper : IMemberMappingMapperExpression, ITypeMappingMapperExpression
    {
#if DEBUG
        private static void debug( object o ) => Console.WriteLine( o );

        protected static readonly Expression<Action<object>> debugExp =
            ( o ) => debug( o );
#endif

        public virtual bool CanHandle( TypeMapping mapping )
        {
            return this.CanHandle( mapping.TypePair.SourceType,
                mapping.TypePair.TargetType );
        }

        public virtual bool CanHandle( MemberMapping mapping )
        {
            var sourceMemberType = mapping.SourceMember.MemberInfo.GetMemberType();
            var targetMemberType = mapping.TargetMember.MemberInfo.GetMemberType();

            return this.CanHandle( sourceMemberType, targetMemberType );
        }

        public abstract bool CanHandle( Type sourceType, Type targetType );

        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            //Action<SourceType, TargetType>
            var context = this.GetContext( mapping );
            var valueAssignment = this.GetTargetValueAssignment( context );

            var body = (Expression)Expression.Block
            (
                new[] { context.TargetMember },

                valueAssignment.ReplaceParameter( context.SourceInstance, "sourceInstance" ),
                context.TargetMemberValueSetter
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceInstanceType,
                context.TargetInstanceType );

            return Expression.Lambda( delegateType, body, context.ReferenceTrack,
                context.SourceInstance, context.TargetInstance );
        }

        public LambdaExpression GetMappingExpression( TypeMapping mapping )
        {
            var context = this.GetContext( mapping );
            return GetMappingExpression( context, mapping.TypePair.SourceType,
                mapping.TypePair.TargetType );
        }

        private LambdaExpression GetMappingExpression( MapperContext context, Type sourceType, Type targetType )
        {
            var targetValueAssignment = this.GetTargetValueAssignment( context );

            var body = Expression.Block
            (
                new[] { context.TargetMember },

                targetValueAssignment,

                //return the value assigned to TargetValue param
                context.TargetMember
            );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType,
                body, context.SourceInstance );
        }

        protected virtual MapperContext GetContext( MemberMapping mapping )
        {
            return new MapperContext( mapping );
        }

        protected virtual MapperContext GetContext( TypeMapping mapping )
        {
            return new MapperContext( mapping );
        }

        protected abstract Expression GetTargetValueAssignment( MapperContext context );
    }
}
