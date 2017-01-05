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
    *- Stack<T> and other LIFO collections require the list to be read in reverse  
    * to preserve order and have a specular clone. This is done with Stack<T> by
    * creating the list two times: 'new Stack( new Stack( sourceCollection ) )'
    * 
    */
    public class StackMapper : CollectionMapper
    {
        public override bool CanHandle( PropertyMapping mapping )
        {
            return base.CanHandle( mapping ) && mapping.TargetProperty.PropertyInfo
                .PropertyType.GetGenericTypeDefinition() == typeof( Stack<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetCollectionType.GetMethod( "Push" );
        }

        protected override Expression GetInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            var addMethod = GetTargetCollectionAddMethod( context );
            var constructor = GetTargetCollectionConstructorFromCollection( context );

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

                //double contructor to read in reverse order
                var targetCollectionConstructor = Expression.New( constructorInfo,
                   Expression.New( constructorInfo, context.SourceCollection ) );

                return Expression.Assign( context.TargetCollection, targetCollectionConstructor );
            }

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();
            var newElement = Expression.Variable( context.TargetElementType, "newElement" );

            //avoids add calls on the Stack because the source and target collection
            //will not be identical (one will be reversed).
            //avoids add calls by creating a temporary list.

            var tempCollectionType = typeof( Stack<> ).MakeGenericType( context.TargetElementType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );
            var tempCollectionAddMethod = this.GetTargetCollectionAddMethod( context );

            var constructorWithCapacity = tempCollectionType.GetConstructor( new Type[] { typeof( int ) } );
            var getCountMethod = context.SourceCollectionType.GetProperty( "Count" ).GetGetMethod();

            var newTempCollectionExp = Expression.New( constructorWithCapacity,
                Expression.Call( context.SourceCollection, getCountMethod ) );

            return Expression.Block
            (
                new[] { newElement, tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar, Expression.Block
                (
                    Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                    Expression.Call( tempCollection, tempCollectionAddMethod, newElement ),

                    Expression.Call( context.NewRefObjects, addToRefCollectionMethod,
                        Expression.New( objectPairConstructor, context.SourceLoopingVar, newElement ) )
                ) ),

                Expression.Assign( context.TargetCollection, Expression.New( constructor, tempCollection ) )
            );
        }
    }
}
