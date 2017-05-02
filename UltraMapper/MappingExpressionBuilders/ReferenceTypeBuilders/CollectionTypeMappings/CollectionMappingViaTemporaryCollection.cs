using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.MappingExpressionBuilders
{
    public abstract class CollectionMappingViaTemporaryCollection : CollectionMapper
    {
        public CollectionMappingViaTemporaryCollection( Configuration configuration )
            : base( configuration ) { }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            //1. Create a new temporary collection passing source as input
            //2. Read items from the newly created temporary collection and add items to the target

            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.SourceCollectionElementType ) };

            var tempCollectionType = this.GetTemporaryCollectionType( context );
            var tempCollectionConstructorInfo = tempCollectionType.GetConstructor( paramType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );

            var newTempCollectionExp = Expression.New( tempCollectionConstructorInfo, context.SourceInstance );

            if( context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    new[] { tempCollection },

                    Expression.Assign( tempCollection, newTempCollectionExp ),
                    SimpleCollectionLoop( context, tempCollection, context.TargetInstance )
                );
            }

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                CollectionLoopWithReferenceTracking( context, tempCollection, context.TargetInstance )
            );
        }

        protected virtual Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( List<> ).MakeGenericType( context.SourceCollectionElementType );
        }
    }
}
