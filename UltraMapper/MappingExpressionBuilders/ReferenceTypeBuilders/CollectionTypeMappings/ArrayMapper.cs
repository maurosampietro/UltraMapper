using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;
            var targetCollectionInsertionMethod = GetTargetCollectionInsertionMethod( context );

            if( context.IsSourceElementTypeBuiltIn || context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block( SimpleCollectionLoop
                (
                    context.SourceInstance, context.SourceCollectionElementType,
                    context.TargetInstance, context.TargetCollectionElementType,
                    targetCollectionInsertionMethod,
                    context.SourceCollectionLoopingVar
                ) );
            }

            return Expression.Block( ComplexCollectionLoop
            (
                context.SourceInstance, context.SourceCollectionElementType,
                context.TargetInstance, context.TargetCollectionElementType,
                targetCollectionInsertionMethod,
                context.SourceCollectionLoopingVar,
                context.ReferenceTracker,
                context.Mapper
            ) );
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
            if( context.Options.ReferenceBehavior == ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL )
            {
                //It's up to the user to ensure that the target instance has enough room 
                //to hold all the elements. We don't check Source.Length <= Target.Length
                return base.GetMemberNewInstance( context );
            }

            var newInstanceWithReservedCapacity = this.GetNewInstanceWithReservedCapacity( context );
            return Expression.Assign( context.TargetMember, newInstanceWithReservedCapacity );
        }
    }
}
