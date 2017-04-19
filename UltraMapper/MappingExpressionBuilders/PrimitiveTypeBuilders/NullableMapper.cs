using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class NullableMapper : PrimitiveMapperBase
    {
        public NullableMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsNullable() || target.IsNullable();
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            if( context.SourceInstance.Type == context.TargetInstance.Type )
                return context.SourceInstance;

            if( context.SourceInstance.Type.IsNullable() && context.TargetInstance.Type.IsNullable() )
            {
                var sourceUnderlyingType = Nullable.GetUnderlyingType( context.SourceInstance.Type );
                var targetUnderlyingType = Nullable.GetUnderlyingType( context.TargetInstance.Type );

                //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceInstance,
                    context.SourceInstance.Type.GetProperty( nameof( Nullable<int>.Value ) ) );

                var convert = MapperConfiguration[ sourceUnderlyingType, targetUnderlyingType ].MappingExpression;

                var constructor = context.TargetInstance.Type.GetConstructor( new Type[] { targetUnderlyingType } );
                var newNullable = Expression.New( constructor, Expression.Invoke( convert, nullableValueAccess ) );

                return Expression.Block
                (
                    new[] { context.TargetInstance },

                    Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, newNullable )
                    ),

                    context.TargetInstance
                );
            }

            if( context.SourceInstance.Type.IsNullable() && !context.TargetInstance.Type.IsNullable() )
            {
                //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
                var nullableValueAccess = Expression.MakeMemberAccess( context.SourceInstance,
                    context.SourceInstance.Type.GetProperty( nameof( Nullable<int>.Value ) ) );

                var sourceUnderlyingType = context.SourceInstance.Type.GetUnderlyingTypeIfNullable();
                if( sourceUnderlyingType == context.TargetInstance.Type )
                {
                    return Expression.Block
                    (
                        new[] { context.TargetInstance },

                        Expression.IfThenElse
                        (
                            Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                            Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                            Expression.Assign( context.TargetInstance, nullableValueAccess )
                        ),

                        context.TargetInstance
                    );
                }

                if( sourceUnderlyingType.IsImplicitlyConvertibleTo( context.TargetInstance.Type ) ||
                    sourceUnderlyingType.IsExplicitlyConvertibleTo( context.TargetInstance.Type ) )
                {
                    return Expression.Block
                    (
                        new[] { context.TargetInstance },

                        Expression.IfThenElse
                        (
                            Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                            Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                            Expression.Assign( context.TargetInstance, Expression.Convert( nullableValueAccess, context.TargetInstance.Type ) )
                        ),

                        context.TargetInstance
                    );
                }

                var convertMethod = typeof( Convert ).GetMethod( $"To{context.TargetInstance.Type.Name}", new[] { context.SourceInstance.Type } );
                return Expression.Block
                (
                    new[] { context.TargetInstance },

                    Expression.IfThenElse
                    (
                        Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, Expression.Default( context.TargetInstance.Type ) ),
                        Expression.Assign( context.TargetInstance, Expression.Call( convertMethod, nullableValueAccess ) )
                    ),

                    context.TargetInstance
                 );
            }

            if( !context.SourceInstance.Type.IsNullable() && context.TargetInstance.Type.IsNullable() )
            {
                var targetUnderlyingType = context.TargetInstance.Type.GetUnderlyingTypeIfNullable();

                if( context.SourceInstance.Type.IsImplicitlyConvertibleTo( targetUnderlyingType ) ||
                  context.SourceInstance.Type.IsExplicitlyConvertibleTo( targetUnderlyingType ) )
                {
                    var constructor = context.TargetInstance.Type.GetConstructor( new Type[] { targetUnderlyingType } );
                    return Expression.New( constructor, Expression.Convert( context.SourceInstance, targetUnderlyingType ) );
                }
                else
                {
                    var constructor = context.TargetInstance.Type.GetConstructor( new Type[] { context.SourceInstance.Type } );
                    return Expression.New( constructor, context.SourceInstance );
                }
            }

            throw new Exception( $"Cannot handle {context.SourceInstance} -> {context.TargetInstance}" );
        }
    }
}
