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
        public override bool CanHandle( PropertyMapping mapping )
        {
            return base.CanHandle( mapping ) && mapping.TargetProperty.PropertyInfo
                .PropertyType.ImplementsInterface( typeof( ISet<> ) );
        }

        protected override Expression GetInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            var addMethod = GetTargetCollectionAddMethod( context );

            if( context.IsTargetElementTypeBuiltIn )
            {
                var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
                if( constructorInfo == null )
                {
                    Expression loopBody = Expression.Call( context.TargetCollection,
                        addMethod, context.SourceLoopingVar );

                    return ExpressionLoops.ForEach( context.SourceCollection,
                        context.SourceLoopingVar, loopBody );
                }

                var targetCollectionConstructor = Expression.New(
                    constructorInfo, context.SourceCollection );

                return Expression.Assign( context.TargetCollection, targetCollectionConstructor );
            }

            var addRangeToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
            var newElement = Expression.Variable( context.TargetElementType, "newElement" );
            var newInstanceExp = Expression.New( context.TargetCollectionType );

            var itemMapping = mapping.TypeMapping.GlobalConfiguration.Configurator[
                context.SourceElementType, context.TargetElementType ].MappingExpression;

            return Expression.Block
            (
                 new[] { newElement },

                 Expression.Assign( context.TargetCollection, newInstanceExp ),
                 ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar, Expression.Block
                 (
                     Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                     Expression.Call( context.NewRefObjects, addRangeToRefCollectionMethod, Expression.Invoke(
                         itemMapping, context.ReferenceTrack, context.SourceLoopingVar, newElement ) ),

                     Expression.Call( context.TargetCollection, addMethod, newElement )
                 ) )
            );
        }
    }
}
