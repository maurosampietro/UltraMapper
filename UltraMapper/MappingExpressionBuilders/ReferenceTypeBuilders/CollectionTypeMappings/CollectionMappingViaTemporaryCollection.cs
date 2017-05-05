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
            var temporaryCollectionInsertionMethod = this.GetTargetCollectionInsertionMethod( context );

            if( context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    new[] { tempCollection },

                    Expression.Assign( tempCollection, newTempCollectionExp ),

                    SimpleCollectionLoop
                    (
                        tempCollection,
                        context.SourceCollectionElementType,
                        context.TargetInstance,
                        context.TargetCollectionElementType,
                        temporaryCollectionInsertionMethod,
                        context.SourceCollectionLoopingVar
                    )
                );
            }

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),

                CollectionLoopWithReferenceTracking
                (
                    tempCollection,
                    context.SourceCollectionElementType,
                    context.TargetInstance,
                    context.TargetCollectionElementType,
                    temporaryCollectionInsertionMethod,
                    context.SourceCollectionLoopingVar,
                    context.ReferenceTracker,
                    context.Mapper
                )
            );
        }

        protected virtual MethodInfo GetTemporaryCollectionInsertionMethod( CollectionMapperContext context )
        {
            return this.GetTemporaryCollectionType( context ).GetMethod( nameof( List<int>.Add ) );
        }

        protected virtual Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( List<> ).MakeGenericType( context.SourceCollectionElementType );
        }

        //protected override Expression GetNewTargetInstanceExpression( MemberMappingContext context )
        //{
        //    var constructorWithInputCollection = context.TargetMember.Type.GetConstructors()
        //        .FirstOrDefault( ctor =>
        //        {
        //            var parameters = ctor.GetParameters();
        //            if( parameters.Length != 1 ) return false;

        //            return parameters[ 0 ].ParameterType.IsEnumerable();
        //        } );

        //    return Expression.New( constructorWithInputCollection, context.SourceMember );
        //}
    }
}
