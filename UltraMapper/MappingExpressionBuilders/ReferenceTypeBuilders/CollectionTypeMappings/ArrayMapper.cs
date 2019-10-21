using System;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ArrayMapper : CollectionMapper
    {
        public ArrayMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) && target.IsArray;
        }

        protected override MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            return typeof( Array ).GetMethod( nameof( Array.Clear ),
                BindingFlags.Public | BindingFlags.Static);
        }

        protected override Expression GetTargetCollectionClearExpression( CollectionMapperContext context )
        {
            bool isResetCollection = /*context.Options.ReferenceBehavior == ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL && */
                context.Options.CollectionBehavior == CollectionBehaviors.RESET;

            var clearMethod = GetTargetCollectionClearMethod( context );
            //var lengthProperty = context.TargetInstance.Type.GetProperty( nameof( Array.Length ) );

            return isResetCollection ? Expression.Call( null, clearMethod, context.TargetInstance,
                Expression.Constant( 0, typeof( int ) ), Expression.ArrayLength( context.TargetInstance ) )
                    : (Expression)Expression.Empty();
        }

        protected override Expression SimpleCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar )
        {
            var itemMapping = MapperConfiguration[ sourceCollectionElementType,
                targetCollectionElementType ].MappingExpression;

            var itemIndex = Expression.Parameter( typeof( int ), "itemIndex" );

            return Expression.Block
            (
                new[] { itemIndex },

                ExpressionLoops.ForEach( sourceCollection, sourceCollectionLoopingVar, Expression.Block
                (
                    Expression.Assign( Expression.ArrayAccess( targetCollection, itemIndex ),
                        itemMapping.Body.ReplaceParameter( sourceCollectionLoopingVar, itemMapping.Parameters[ 0 ].Name ) ),

                    Expression.AddAssign( itemIndex, Expression.Constant( 1 ) )
                ) )
            );
        }

        protected override Expression ComplexCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar,
            ParameterExpression referenceTracker, ParameterExpression mapper )
        {
            var newElement = Expression.Variable( targetCollectionElementType, "newElement" );
            var itemIndex = Expression.Parameter( typeof( int ), "itemIndex" );

            return Expression.Block
            (
                new[] { newElement, itemIndex },

                ExpressionLoops.ForEach( sourceCollection, sourceCollectionLoopingVar, Expression.Block
                (
                    LookUpBlock( sourceCollectionLoopingVar, newElement, referenceTracker, mapper ),
                    Expression.Assign( Expression.ArrayAccess( targetCollection, itemIndex ), newElement ),

                    Expression.AddAssign( itemIndex, Expression.Constant( 1 ) )
                )
            ) );
        }

        protected override Expression GetMemberNewInstance( MemberMappingContext context )
        {
            return this.GetNewInstanceWithReservedCapacity( context );
        }
    }
}
