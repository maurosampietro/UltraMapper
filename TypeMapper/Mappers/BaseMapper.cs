using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class BaseMapper : IMapperExpression, IMemberMappingMapperExpression, ITypeMappingMapperExpression
    {
        public virtual bool CanHandle( TypeMapping mapping )
        {
            return this.CanHandle( mapping.TypePair.SourceType,
                mapping.TypePair.TargetType );
        }

        public virtual bool CanHandle( MemberMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            return this.CanHandle( sourcePropertyType, targetPropertyType );
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

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            //Func<SourceType, TargetType>

            var context = this.GetContext( sourceType, targetType );
            return GetMappingExpression( context, sourceType, targetType );
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

        protected virtual MapperContext GetContext( Type sourceType, Type targetType )
        {
            return new MapperContext( sourceType, targetType );
        }

        protected virtual MapperContext GetContext( TypeMapping mapping )
        {
            return new MapperContext( mapping );
        }

        protected abstract Expression GetTargetValueAssignment( MapperContext context );
    }
}
