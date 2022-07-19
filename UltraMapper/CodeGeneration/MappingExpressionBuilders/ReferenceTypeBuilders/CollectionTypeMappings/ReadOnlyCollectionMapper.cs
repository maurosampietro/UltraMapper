using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ReadOnlyCollectionMapper : CollectionMapper
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return base.CanHandle( mapping ) && new Lazy<bool>( () =>
            {
                bool hasTargetDefaultParameterlessCtor = target.EntryType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null ) != null;

                if( hasTargetDefaultParameterlessCtor ) return false;

                bool hasInputCollectionCtor = target.EntryType.GetConstructors().FirstOrDefault( ctor =>
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

        public override Expression GetMemberNewInstance( MemberMappingContext context, out bool isMapComplete )
        {
            isMapComplete = false;
            //1. Create a new temporary collection passing source as input
            //2. Read items from the newly created temporary collection and add items to the target

            var collectionContext = (CollectionMapperContext)GetMapperContext( (Mapping)context.Options );

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
                        collectionContext.SourceCollectionLoopingVar,
                        collectionContext.Mapper,
                        collectionContext.ReferenceTracker, 
                        collectionContext
                    ),

                    Expression.New( newTargetCtor, tempCollection )
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
                    context.Mapper,
                    collectionContext
                ),

                Expression.New( newTargetCtor, tempCollection )
            );
        }
    }
}