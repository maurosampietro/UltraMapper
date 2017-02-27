using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        public override bool CanHandle( MemberMapping mapping )
        {
            return base.CanHandle( mapping ) && mapping.TargetProperty.MemberInfo
                .GetMemberType().GetGenericTypeDefinition() == typeof( Stack<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetMemberType.GetMethod( "Push" );
        }

        protected override Expression GetComplexTypeInnerBody( MemberMapping mapping, CollectionMapperContext context )
        {
            var addMethod = GetTargetCollectionAddMethod( context );
            var constructorInfo = GetTargetCollectionConstructorFromCollection( context );

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();
            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );

            //avoids add calls on the Stack because the source and target collection
            //will not be identical (one will be reversed).
            //avoids add calls by creating a temporary list.

            var tempCollectionType = typeof( Stack<> ).MakeGenericType( context.TargetCollectionElementType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );
            var tempCollectionAddMethod = this.GetTargetCollectionAddMethod( context );

            var constructorWithCapacity = tempCollectionType.GetConstructor( new Type[] { typeof( int ) } );
            var getCountMethod = context.SourceMemberType.GetProperty( "Count" ).GetGetMethod();

            var newTempCollectionExp = Expression.New( constructorWithCapacity,
                Expression.Call( context.SourceMember, getCountMethod ) );

            return Expression.Block
            (
                new[] { newElement, tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                (
                    Expression.Assign( newElement, Expression.New( context.TargetCollectionElementType ) ),
                    Expression.Call( tempCollection, tempCollectionAddMethod, newElement ),

                    Expression.Call( context.ReturnObject, addToRefCollectionMethod,
                        Expression.New( objectPairConstructor, context.SourceCollectionLoopingVar, newElement ) )
                ) ),

                Expression.Assign( context.TargetMember, Expression.New( constructorInfo, tempCollection ) )
            );
        }
    }
}
