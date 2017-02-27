using System.Collections.Generic;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    /*NOTES:
     * 
     *- SortedSet<T> need immediate recursion
     *in order for the item to be added to the collection if T is a 
     *complex type that implements IComparable<T> 
     * 
     * - HashSet<T> need immediate recursion
     *in order for the item to be added to the collection if T is a 
     *complex type that overrides GetHashCode and Equals
     * 
     */

    /// <summary>
    ///Sets need the items to be mapped before adding them to the collection;
    ///otherwise all sets will contain only one item (the one created by the default constructor)
    /// </summary>
    public class SetMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            var memberType = mapping.TargetProperty.MemberInfo.GetMemberType();
            return base.CanHandle( mapping ) && memberType.IsGenericType
                && memberType.ImplementsInterface( typeof( ISet<> ) );
        }

        protected override Expression GetComplexTypeInnerBody( MemberMapping mapping, CollectionMapperContext context )
        {
            var addMethod = GetTargetCollectionAddMethod( context );

            var addRangeToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );
            var newInstanceExp = Expression.New( context.TargetMemberType );

            var itemMapping = context.Mapping.TypeMapping.GlobalConfiguration.Configurator[
                context.SourceCollectionElementType, context.TargetCollectionElementType ].MappingExpression;

            Expression lookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, context.ReferenceTrack, context.SourceCollectionLoopingVar,
                    Expression.Constant( context.TargetCollectionElementType ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, context.ReferenceTrack, context.SourceCollectionLoopingVar,
                Expression.Constant( context.TargetCollectionElementType ), newElement );

            return Expression.Block
            (
                 new[] { newElement },

                 Expression.Assign( context.TargetMember, newInstanceExp ),
                 ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                 (
                     Expression.Assign( newElement, Expression.Convert( lookupCall, context.TargetCollectionElementType ) ),
                     Expression.IfThen
                     (
                         Expression.Equal( newElement, Expression.Constant( null, context.TargetCollectionElementType ) ),
                         Expression.Block
                         (
                             Expression.Assign( newElement, Expression.New( context.TargetCollectionElementType ) ),
                     
                             //cache new collection
                             addToLookupCall,
                     
                             Expression.Call( context.ReturnObject, addRangeToRefCollectionMethod, Expression.Invoke(
                                 itemMapping, context.ReferenceTrack, context.SourceCollectionLoopingVar, newElement ) )
                         )
                     ),
                     
                     Expression.Call( context.TargetMember, addMethod, newElement )
                 ) )
            );
        }
    }
}
