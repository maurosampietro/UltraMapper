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
                return Expression.Block
                (
                    SimpleCollectionLoop
                    (
                        context.SourceInstance, context.SourceCollectionElementType,
                        context.TargetInstance, context.TargetCollectionElementType,
                        targetCollectionInsertionMethod,
                        context.SourceCollectionLoopingVar
                    )
                );
            }

            return Expression.Block
            (
                ComplexCollectionLoop
                (
                    context.SourceInstance, context.SourceCollectionElementType,
                    context.TargetInstance, context.TargetCollectionElementType,
                    targetCollectionInsertionMethod,
                    context.SourceCollectionLoopingVar,
                    context.ReferenceTracker,
                    context.Mapper
                )
            );
        }

        //protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        //{
        //    //'Item' is the default name of an indexer
        //    return context.TargetInstance.Type.GetMethod( "SetValue", BindingFlags.Instance | BindingFlags.Public,
        //        null, new Type[] { context.TargetCollectionElementType, typeof( int ) }, null );
        //}

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

        public override Expression ComplexCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
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

        protected override Expression GetNewTargetInstance( MemberMappingContext context )
        {
            //if( context.Options.ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
            //{
            //    if( context.SourceMember.Count < context.TargetMember.Count )
            //        return base.GetNewTargetInstance( context );
            //}

            var constructorWithCapacity = context.TargetMember.Type.GetConstructor( new Type[] { typeof( int ) } );

            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            var getCountProperty = context.SourceMember.Type.GetProperty( nameof( ICollection<int>.Count ),
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public );

            if( getCountProperty == null )
            {
                //ICollection<T> interface implementation is injected in the Array class at runtime.
                //Array implements ICollection.Count explicitly. 
                //For simplicity, we just look for property Length :)
                getCountProperty = context.SourceMember.Type.GetProperty( nameof( Array.Length ) );
            }

            var getCountMethod = getCountProperty.GetGetMethod();

            return Expression.Assign( context.TargetMember, Expression.New( constructorWithCapacity,
                Expression.Call( context.SourceMember, getCountMethod ) ) );
        }
    }
}
