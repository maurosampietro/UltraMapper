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
        public StackMapper( MapperConfiguration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) &&
                target.GetGenericTypeDefinition() == typeof( Stack<> );
        }

        protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            return context.TargetInstance.Type.GetMethod( "Push" );
        }

        protected override Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            //1. Reverse the Stack by creating a new temporary Stack( sourceInstance )
            //2. Read items from temporary stack and add items to the target instance

            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.SourceCollectionElementType ) };

            var tempCollectionType = typeof( Stack<> ).MakeGenericType( context.SourceCollectionElementType );
            var tempCollectionConstructorInfo = tempCollectionType.GetConstructor( paramType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );

            var newTempCollectionExp = Expression.New( tempCollectionConstructorInfo, context.SourceInstance );

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                SimpleCollectionLoop( context, tempCollection, context.TargetInstance )
            );
        }

        protected override Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            //1. Reverse the Stack by creating a new temporary Stack( sourceInstance )
            //2. Read items from temporary stack and add items to the target instance

            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.SourceCollectionElementType ) };

            var tempCollectionType = typeof( Stack<> ).MakeGenericType( context.SourceCollectionElementType );
            var tempCollectionConstructorInfo = tempCollectionType.GetConstructor( paramType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );

            var newTempCollectionExp = Expression.New( tempCollectionConstructorInfo, context.SourceInstance );

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                CollectionLoopWithReferenceTracking( context, tempCollection, context.TargetInstance )
            );
        }
    }
}
