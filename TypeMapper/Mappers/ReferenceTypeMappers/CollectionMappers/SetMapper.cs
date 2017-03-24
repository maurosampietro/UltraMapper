//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using TypeMapper.Internals;

//namespace TypeMapper.Mappers
//{
//    /*NOTES:
//     * 
//     *- SortedSet<T> need immediate recursion
//     *in order for the item to be added to the collection if T is a 
//     *complex type that implements IComparable<T> 
//     * 
//     * - HashSet<T> need immediate recursion
//     *in order for the item to be added to the collection if T is a 
//     *complex type that overrides GetHashCode and Equals
//     * 
//     */

//    /// <summary>
//    ///Sets need the items to be mapped before adding them to the collection;
//    ///otherwise all sets will contain only one item (the one created by the default constructor)
//    /// </summary>
//    public class SetMapper : CollectionMapper
//    {
//        public SetMapper( MapperConfiguration configuration )
//            : base( configuration ) { }

//        public override bool CanHandle( Type source, Type target )
//        {
//            return base.CanHandle( source, target ) && 
//                target.ImplementsInterface( typeof( ISet<> ) );
//        }

//        protected override Expression GetComplexTypeInnerBody( CollectionMapperContext context )
//        {
//            var addMethod = GetTargetCollectionAddMethod( context );

//            var addRangeToRefCollectionMethod = typeof( List<ObjectPair> ).GetMethod( nameof( List<ObjectPair>.AddRange ) );
//            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );
//            var newInstanceExp = Expression.New( context.TargetInstance.Type );

//            var itemMapping = MapperConfiguration[ context.SourceCollectionElementType, 
//                context.TargetCollectionElementType ].MappingExpression;

//            return Expression.Block
//            (
//                 new[] { newElement },

//                 Expression.Assign( context.TargetInstance, newInstanceExp ),
//                 ExpressionLoops.ForEach( context.SourceInstance, context.SourceCollectionLoopingVar, Expression.Block
//                 (
//                     Expression.Assign( newElement, Expression.New( context.TargetCollectionElementType ) ),
//                     Expression.Call( context.ReturnObject, addRangeToRefCollectionMethod, Expression.Invoke(
//                         itemMapping, context.ReferenceTrack, context.SourceCollectionLoopingVar, newElement ) ),

//                     Expression.Call( context.TargetInstance, addMethod, newElement )
//                 ) )
//            );
//        }
//    }
//}
