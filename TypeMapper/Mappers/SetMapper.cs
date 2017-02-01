using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            return base.CanHandle( mapping ) && mapping.TargetProperty.MemberInfo
                .GetMemberType().ImplementsInterface( typeof( ISet<> ) );
        }

        protected override Expression GetComplexTypeInnerBody( MemberMapping mapping, CollectionMapperContext context )
        {
            var addMethod = GetTargetCollectionAddMethod( context );
   
            var addRangeToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
            var newElement = Expression.Variable( context.TargetElementType, "newElement" );
            var newInstanceExp = Expression.New( context.TargetPropertyType );

            var itemMapping = context.Mapping.TypeMapping.GlobalConfiguration.Configurator[
                context.SourceElementType, context.TargetElementType ].MappingExpression;

            return Expression.Block
            (
                 new[] { newElement },

                 Expression.Assign( context.TargetPropertyVar, newInstanceExp ),
                 ExpressionLoops.ForEach( context.SourcePropertyVar, context.SourceLoopingVar, Expression.Block
                 (
                     Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                     Expression.Call( context.ReturnObjectVar, addRangeToRefCollectionMethod, Expression.Invoke(
                         itemMapping, context.ReferenceTrack, context.SourceLoopingVar, newElement ) ),

                     Expression.Call( context.TargetPropertyVar, addMethod, newElement )
                 ) )
            );
        }
    }
}
