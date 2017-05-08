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
    public class ReadOnlyCollectionMapper : CollectionMapper
    {
        public ReadOnlyCollectionMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) && new Lazy<bool>( () =>
            {
                bool hasTargetDefaultParameterlessCtor = target.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null ) != null;

                if( hasTargetDefaultParameterlessCtor ) return false;

                bool hasInputCollectionCtor = target.GetConstructors().FirstOrDefault( ctor =>
                {
                    var parameters = ctor.GetParameters();
                    if( parameters.Length != 1 ) return false;

                    return parameters[ 0 ].ParameterType.IsEnumerable();
                } ) != null;

                return hasInputCollectionCtor;
            } ).Value;
        }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            //var context = contextObj as CollectionMapperContext;

            ////1. Create a new temporary collection passing source as input
            ////2. Read items from the newly created temporary collection and add items to the target

            ////var paramType = new Type[] { typeof( int ) };

            //var tempCollectionType = this.GetTemporaryCollectionType( context );
            //var tempCollectionConstructorInfo = tempCollectionType.GetConstructor( Type.EmptyTypes );
            //var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );

            //var newTempCollectionExp = Expression.New( tempCollectionConstructorInfo );

            //var newTargetCtor = context.TargetInstance.Type.GetConstructors().First( ctor =>
            //{
            //    var parameters = ctor.GetParameters();
            //    if( parameters.Length != 1 ) return false;

            //    return parameters[ 0 ].ParameterType.IsEnumerable();
            //} );

            //if( context.IsTargetElementTypeBuiltIn )
            //{
            //    return Expression.Block
            //    (
            //        new[] { tempCollection },

            //        SimpleCollectionLoop( context.SourceInstance, context.SourceCollectionElementType, tempCollection, context.TargetCollectionElementType,
            //            this.GetTemporaryCollectionInsertionMethod( context ), context.SourceCollectionLoopingVar ),

            //        Expression.Assign( context.TargetInstance, Expression.New( newTargetCtor, tempCollection ) )
            //    );
            //}

            //return Expression.Block
            //(
            //    new[] { tempCollection },

            //    Expression.Assign( tempCollection, newTempCollectionExp ),
            //    CollectionLoopWithReferenceTracking( context, tempCollection, context.TargetInstance )
            //);

            return Expression.Empty();
        }

        protected virtual MethodInfo GetTemporaryCollectionInsertionMethod( CollectionMapperContext context )
        {
            return this.GetTemporaryCollectionType( context ).GetMethod( nameof( List<int>.Add ) );
        }

        protected virtual Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( List<> ).MakeGenericType( context.SourceCollectionElementType );
        }

        public override Expression GetTargetInstanceAssignment( MemberMappingContext context )
        {
            //1. Create a new temporary collection passing source as input
            //2. Read items from the newly created temporary collection and add items to the target

            //var paramType = new Type[] { typeof( int ) };

            var collectionContext = (CollectionMapperContext)
                GetMapperContext( context.SourceMember.Type, context.TargetMember.Type, context.Options );

            var tempCollectionType = this.GetTemporaryCollectionType( collectionContext );
            var tempCollectionConstructorInfo = tempCollectionType.GetConstructor( Type.EmptyTypes );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );

            var newTempCollectionExp = Expression.New( tempCollectionConstructorInfo );

            var newTargetCtor = context.TargetMember.Type.GetConstructors().First( ctor =>
            {
                var parameters = ctor.GetParameters();
                if( parameters.Length != 1 ) return false;

                return parameters[ 0 ].ParameterType.IsEnumerable();
            } );

            var temporaryCollectionInsertionMethod = this.GetTemporaryCollectionInsertionMethod( collectionContext );
            if( collectionContext.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    new[] { tempCollection },
                    Expression.Assign( tempCollection, newTempCollectionExp ),

                    SimpleCollectionLoop
                    (
                        context.SourceMember,
                        collectionContext.SourceCollectionElementType,
                        tempCollection,
                        collectionContext.TargetCollectionElementType,
                        temporaryCollectionInsertionMethod,
                        collectionContext.SourceCollectionLoopingVar
                    ),

                    Expression.Assign( context.TargetMember, Expression.New( newTargetCtor, tempCollection ) )
                );
            }

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),

                ComplexCollectionLoop
                (
                    context.SourceMember,
                    collectionContext.SourceCollectionElementType,
                    tempCollection,
                    collectionContext.TargetCollectionElementType,
                    temporaryCollectionInsertionMethod,
                    collectionContext.SourceCollectionLoopingVar,
                    context.ReferenceTracker,
                    context.Mapper
                ),

                Expression.Assign( context.TargetMember, Expression.New( newTargetCtor, tempCollection ) )
            );
        }
    }
}
