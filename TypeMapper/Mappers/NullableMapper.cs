using System;
using System.Linq.Expressions;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class NullableMapper : BaseMapper, IMemberMappingMapperExpression, IMapperExpression, ITypeMappingMapperExpression
    {
        public override bool CanHandle( Type source, Type target )
        {
            return source.IsNullable() || target.IsNullable();
        }

        protected override Expression GetTargetValueAssignment( MapperContext context )
        {
            if( context.SourceMemberType == context.TargetMemberType )
                return Expression.Assign( context.TargetMember, context.SourceMemberValue );

            if( context.SourceMemberType.IsNullable() && context.TargetMemberType.IsNullable() )
            {
                var sourceUnderlyingType = Nullable.GetUnderlyingType( context.SourceMemberType );
                var targetUnderlyingType = Nullable.GetUnderlyingType( context.TargetMemberType );

                //Nullable<int> is used only because it is forbidden to use nameof with open generics.
                //Any other struct type instead of int would work.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceInstance,
                    context.SourceMemberType.GetProperty( nameof( Nullable<int>.Value ) ) );

                var typeMapping = context.MapperConfiguration.Configurator[
                     sourceUnderlyingType, targetUnderlyingType ];

                var convert = typeMapping.MappingExpression;

                var constructor = context.TargetMemberType.GetConstructor( new Type[] { targetUnderlyingType } );
                var newNullable = Expression.New( constructor, Expression.Invoke( convert, nullableValueAccess ) );

                return Expression.IfThenElse
                (
                    Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceMemberType ) ),
                    Expression.Assign( context.TargetMember, Expression.Default( context.TargetMemberType ) ),
                    Expression.Assign( context.TargetMember, newNullable )
                );
            }

            if( context.SourceMemberType.IsNullable() && !context.TargetMemberType.IsNullable() )
            {
                //Nullable<int> is used only because it is forbidden to use nameof with open generics.
                //Any other struct type instead of int would work.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceMemberValue,
                    context.SourceMemberType.GetProperty( nameof( Nullable<int>.Value ) ) );

                var sourceUnderlyingType = context.SourceMemberType.GetUnderlyingTypeIfNullable();
                if( sourceUnderlyingType == context.TargetMemberType )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceMemberValue, Expression.Constant( null, context.SourceMemberType ) ),
                        Expression.Assign( context.TargetMember, Expression.Default( context.TargetMemberType ) ),
                        Expression.Assign( context.TargetMember, nullableValueAccess )
                    );
                }

                if( sourceUnderlyingType.IsImplicitlyConvertibleTo( context.TargetMemberType ) ||
                    sourceUnderlyingType.IsExplicitlyConvertibleTo( context.TargetMemberType ) )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceMemberValue, Expression.Constant( null, context.SourceMemberType ) ),
                        Expression.Assign( context.TargetMember, Expression.Default( context.TargetMemberType ) ),
                        Expression.Assign( context.TargetMember, Expression.Convert( nullableValueAccess, context.TargetMemberType ) )
                    );
                }

                var convertMethod = typeof( Convert ).GetMethod( $"To{context.TargetMemberType.Name}", new[] { context.SourceMemberType } );
                return Expression.IfThenElse
                (
                    Expression.Equal( context.SourceMemberValue, Expression.Constant( null, context.SourceMemberType ) ),
                    Expression.Assign( context.TargetMember, Expression.Default( context.TargetMemberType ) ),
                    Expression.Assign( context.TargetMember, Expression.Call( convertMethod, nullableValueAccess ) )
                );
            }

            if( !context.SourceMemberType.IsNullable() && context.TargetMemberType.IsNullable() )
            {
                var targetUnderlyingType = context.TargetMemberType.GetUnderlyingTypeIfNullable();

                if( context.SourceMemberType.IsImplicitlyConvertibleTo( targetUnderlyingType ) ||
                  context.SourceMemberType.IsExplicitlyConvertibleTo( targetUnderlyingType ) )
                {
                    var constructor = context.TargetMemberType.GetConstructor( new Type[] { targetUnderlyingType } );
                    var newNullable = Expression.New( constructor, Expression.Convert( context.SourceMemberValue, targetUnderlyingType ) );
                    return Expression.Assign( context.TargetMember, newNullable );
                }
                else
                {
                    var constructor = context.TargetMemberType.GetConstructor( new Type[] { context.SourceMemberType } );
                    var newNullable = Expression.New( constructor, context.SourceMemberValue );
                    return Expression.Assign( context.TargetMember, newNullable );
                }
            }

            throw new Exception( $"Cannot handle {context.SourceMemberValue} -> {context.TargetMember}" );
        }
    }
}
