using System;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Internals.ExtensionMethods;
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
                BindingFlags.Public | BindingFlags.Static );
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

        private object runtimeMappingInterfaceToPrimitiveType( object loopingvar, Type targetType )
        {
            var map = this.MapperConfiguration[ loopingvar.GetType(), targetType ];
            return map.MappingFuncPrimitives( null, loopingvar );
        }

        protected override Expression SimpleCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar, ParameterExpression mapper, ParameterExpression referenceTracker )
        {
            if( sourceCollectionElementType.IsInterface )
            {
                Expression<Func<object, Type, object>> getRuntimeMapping =
                   ( loopingvar, targetType ) => runtimeMappingInterfaceToPrimitiveType( loopingvar, targetType );

                var itemIndex = Expression.Parameter( typeof( int ), "itemIndex" );
                var newElement = Expression.Variable( targetCollectionElementType, "newElement" );

                Expression loopBody = Expression.Block
                (
                    new[] { newElement, itemIndex },
                    Expression.Assign( itemIndex, Expression.Constant( 0 ) ),

                    ExpressionLoops.ForEach( sourceCollection, sourceCollectionLoopingVar, Expression.Block
                    (
                        Expression.Assign( newElement, Expression.Convert(
                            Expression.Invoke( getRuntimeMapping, sourceCollectionLoopingVar,
                            Expression.Constant( targetCollectionElementType ) ), targetCollectionElementType ) ),

                        Expression.Assign( Expression.ArrayAccess( targetCollection, itemIndex ), newElement ),
                        Expression.AddAssign( itemIndex, Expression.Constant( 1 ) )
                    ) )
                );

                return ExpressionLoops.ForEach( sourceCollection,
                    sourceCollectionLoopingVar, loopBody );
            }
            else
            {
                var itemMapping = MapperConfiguration[ sourceCollectionElementType,
                targetCollectionElementType ].MappingExpression;

                var itemIndex = Expression.Parameter( typeof( int ), "itemIndex" );

                return Expression.Block
                (
                    new[] { itemIndex },

                    Expression.Assign( itemIndex, Expression.Constant( 0 ) ),

                    ExpressionLoops.ForEach( sourceCollection, sourceCollectionLoopingVar, Expression.Block
                    (
                        Expression.Assign( Expression.ArrayAccess( targetCollection, itemIndex ),
                            itemMapping.Body.ReplaceParameter( sourceCollectionLoopingVar, "sourceInstance" ) ),

                        Expression.AddAssign( itemIndex, Expression.Constant( 1 ) )
                    ) )
                );
            }
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

        public override Expression GetMemberNewInstance( MemberMappingContext context )
        {
            return this.GetNewInstanceWithReservedCapacity( context );
        }

        public override Expression GetMemberAssignment( MemberMappingContext context )
        {
            //FOR ARRAYS WE ALSO CHECK IF THE TARGET ARRAY IS LARGE ENOUGH
            //TO HOLD ALL OF THE ITEMS OF THE SOURCE COLLECTION.
            //IF THE ARRAY IS NOT LARGE ENOUGH, WE CREATE A NEW INSTANCE LARGE ENOUGH.

            Expression newInstance = this.GetMemberNewInstance( context );

            var sourceCountMethod = this.GetCountMethod( context.SourceMember.Type );
            var targetCountMethod = this.GetCountMethod( context.TargetMember.Type );

            Expression sourceCountMethodCallExp;
            if( sourceCountMethod.IsStatic )
                sourceCountMethodCallExp = Expression.Call( null, sourceCountMethod, context.SourceMember );
            else sourceCountMethodCallExp = Expression.Call( context.SourceMember, sourceCountMethod );

            Expression targetCountMethodCallExp;
            if( targetCountMethod.IsStatic )
                targetCountMethodCallExp = Expression.Call( null, targetCountMethod, context.TargetMember );
            else
                targetCountMethodCallExp = Expression.Call( context.TargetMember, targetCountMethod );

            return Expression.Block
            (
                base.GetMemberAssignment( context ),

                Expression.IfThen
                (
                    Expression.LessThan( targetCountMethodCallExp, sourceCountMethodCallExp ),
                    Expression.Assign( context.TargetMember, newInstance )
                )
            );
        }

        protected override MethodInfo GetUpdateCollectionMethod( CollectionMapperContext context )
        {
            return typeof( LinqExtensions ).GetMethod
               (
                   nameof( LinqExtensions.ArrayUpdate ),
                   BindingFlags.Static | BindingFlags.Public
               )
               .MakeGenericMethod
               (
                   context.SourceCollectionElementType,
                   context.TargetCollectionElementType
               );
        }
    }
}
