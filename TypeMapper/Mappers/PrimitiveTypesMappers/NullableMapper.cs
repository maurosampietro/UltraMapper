using System;
using System.Linq.Expressions;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class NullableMapper : PrimitiveMapperBase
    {
        public NullableMapper( MapperConfiguration configuration ) 
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsNullable() || target.IsNullable();
        }

        protected override Expression GetTargetValueAssignment( MapperContext context )
        {
            if( context.SourceInstance.Type == context.TargetInstance.Type )
                return Expression.Assign( context.TargetInstance, context.SourceInstance );

            if( context.SourceInstance.Type.IsNullable() && context.TargetInstance.Type.IsNullable() )
            {
                var sourceUnderlyingType = Nullable.GetUnderlyingType( context.SourceInstance.Type );
                var targetUnderlyingType = Nullable.GetUnderlyingType( context.TargetInstance.Type );

                //Nullable<int> is used only because it is forbidden to use nameof with unbound generic types.
                //Any other struct type instead of int would work.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceInstance,
                    context.SourceInstance.Type.GetProperty( nameof( Nullable<int>.Value ) ) );

                var typeMapping = MapperConfiguration[
                     sourceUnderlyingType, targetUnderlyingType ];

                var convert = typeMapping.MappingExpression;

                var constructor = context.TargetInstance.Type.GetConstructor( new Type[] { targetUnderlyingType } );
                var newNullable = Expression.New( constructor, Expression.Invoke( convert, nullableValueAccess ) );

                return Expression.IfThenElse
                (
                    Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                    Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                    Expression.Assign( context.TargetInstance, newNullable )
                );
            }

            if( context.SourceInstance.Type.IsNullable() && !context.TargetInstance.Type.IsNullable() )
            {
                //Nullable<int> is used only because it is forbidden to use nameof with unbound generic types.
                //Any other struct type instead of int would work.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceInstance,
                    context.SourceInstance.Type.GetProperty( nameof( Nullable<int>.Value ) ) );

                var sourceUnderlyingType = context.SourceInstance.Type.GetUnderlyingTypeIfNullable();
                if( sourceUnderlyingType == context.TargetInstance.Type )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, nullableValueAccess )
                    );
                }

                if( sourceUnderlyingType.IsImplicitlyConvertibleTo( context.TargetInstance.Type ) ||
                    sourceUnderlyingType.IsExplicitlyConvertibleTo( context.TargetInstance.Type ) )
                {
                    return Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, Expression.Convert( nullableValueAccess, context.TargetInstance.Type ) )
                    );
                }

                var convertMethod = typeof( Convert ).GetMethod( $"To{context.TargetInstance.Type.Name}", new[] { context.SourceInstance.Type } );
                return Expression.IfThenElse
                (
                    Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                    Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                    Expression.Assign( context.TargetInstance, Expression.Call( convertMethod, nullableValueAccess ) )
                );
            }

            if( !context.SourceInstance.Type.IsNullable() && context.TargetInstance.Type.IsNullable() )
            {
                var targetUnderlyingType = context.TargetInstance.Type.GetUnderlyingTypeIfNullable();

                if( context.SourceInstance.Type.IsImplicitlyConvertibleTo( targetUnderlyingType ) ||
                  context.SourceInstance.Type.IsExplicitlyConvertibleTo( targetUnderlyingType ) )
                {
                    var constructor = context.TargetInstance.Type.GetConstructor( new Type[] { targetUnderlyingType } );
                    var newNullable = Expression.New( constructor, Expression.Convert( context.SourceInstance, targetUnderlyingType ) );
                    return Expression.Assign( context.TargetInstance, newNullable );
                }
                else
                {
                    var constructor = context.TargetInstance.Type.GetConstructor( new Type[] { context.SourceInstance.Type } );
                    var newNullable = Expression.New( constructor, context.SourceInstance );
                    return Expression.Assign( context.TargetInstance, newNullable );
                }
            }

            throw new Exception( $"Cannot handle {context.SourceInstance} -> {context.TargetInstance}" );
        }
    }
}
