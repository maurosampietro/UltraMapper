using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.MappingExpressionBuilders
{
    //enhancements...
    //public abstract class CollectionMappingViaTempCollection : CollectionMapper
    //{
    //    public CollectionMappingViaTempCollection( Configuration configuration )
    //        : base( configuration ) { }

    //    protected override ReferenceMapperContext GetMapperContext( Type source, Type target, IMappingOptions options )
    //    {
    //        return new CollectionMapperViaTempCollectionContext( source, target, options );
    //    }

    //    protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
    //    {
    //        if( !this.IsCopySourceToTempCollection && !this.IsCopyTargetToTempCollection )
    //            return base.GetExpressionBody( contextObj );

    //        //1. Create a new temporary collection passing the source collection as input
    //        //   AND/OR   
    //        //   Create a new temporary collection passing the target collection as input

    //        //2. Read items from the source temp collection and add items to the target collection
    //        //   OR
    //        //   Read items from the source collection, add items to the target temp collection
    //        //   and then build the target collection from the target temp collection
    //        //   OR
    //        //   Read items from the source temp collection and add items to the target temp collection
    //        //   and then build the target collection from the target temp collection

    //        var context = contextObj as CollectionMapperViaTempCollectionContext;

    //        //temp collection for source instance
    //        var tempSourceCollCtorArgType = new Type[] { typeof( IEnumerable<> )
    //            .MakeGenericType( context.SourceCollectionElementType ) };

    //        var tempSourceCollType = this.GetSourceTempCollectionType( context );
    //        var tempSourceCollCtorInfo = tempSourceCollType.GetConstructor( tempSourceCollCtorArgType );
    //        var tempSourceColl = Expression.Parameter( tempSourceCollType, "tempSourceCollection" );

    //        var tempSourceCollExp = Expression.New( tempSourceCollCtorInfo, context.SourceInstance );

    //        //temp collection for target instance
    //        var tempTargetCollCtorArgType = new Type[] { typeof( IEnumerable<> )
    //            .MakeGenericType( context.TargetCollectionElementType ) };

    //        var tempTargetCollType = this.GetTargetTempCollectionType( context );
    //        var tempTargetCollCtorInfo = tempTargetCollType.GetConstructor( tempTargetCollCtorArgType );
    //        var tempTargetColl = Expression.Parameter( tempTargetCollType, "tempTargetCollection" );

    //        var tempTargetCollExp = Expression.New( tempTargetCollCtorInfo, context.TargetInstance );

    //        var tempTargetCollInsertionMethod = this.GetTargetTempCollectionInsertionMethod( context );
    //        var targetCollInsertionMethod = this.GetTargetCollectionInsertionMethod( context );

    //        bool isUpdate = context.Options.CollectionBehavior == CollectionBehaviors.UPDATE;
    //        bool isMergeOrUpdate = context.Options.CollectionBehavior == CollectionBehaviors.MERGE || isUpdate;

    //        IEnumerable<ParameterExpression> getParamsExp()
    //        {
    //            if( this.IsCopySourceToTempCollection )
    //                yield return tempSourceColl;

    //            if( this.IsCopyTargetToTempCollection )
    //                yield return tempTargetColl;
    //        };

    //        var paramsExp = getParamsExp().ToArray();

    //        if( this.IsCopySourceToTempCollection && this.IsCopyTargetToTempCollection )
    //        {
    //            var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo.MakeGenericMethod(
    //                tempSourceColl.Type, tempTargetColl.Type );

    //            if( context.IsTargetElementTypeBuiltIn )
    //            {
    //                return Expression.Block
    //                (
    //                    paramsExp,

    //                    Expression.Assign( tempSourceColl, tempSourceCollExp ),
    //                    Expression.Assign( tempTargetColl, tempTargetCollExp ),

    //                    //copy from temp source to temp target
    //                    SimpleCollectionLoop
    //                    (
    //                        tempSourceColl,
    //                        context.SourceCollectionElementType,
    //                        tempTargetColl,
    //                        context.TargetCollectionElementType,
    //                        tempTargetCollInsertionMethod,
    //                        context.SourceCollectionLoopingVar
    //                    ),

    //                    //copy from temp target to target
    //                    SimpleCollectionLoop
    //                    (
    //                        tempTargetColl,
    //                        context.TargetCollectionElementType,
    //                        context.TargetInstance,
    //                        context.TargetCollectionElementType,
    //                        targetCollInsertionMethod,
    //                        context.TempTargetCollectionLoopingVar
    //                    )
    //                );
    //            }

    //            return Expression.Block
    //            (
    //                paramsExp,

    //                Expression.Assign( tempSourceColl, tempSourceCollExp ),
    //                Expression.Assign( tempTargetColl, tempTargetCollExp ),

    //                ComplexCollectionLoop
    //                (
    //                    tempTargetColl,
    //                    context.TargetCollectionElementType,
    //                    context.TargetInstance,
    //                    context.TargetCollectionElementType,
    //                    targetCollInsertionMethod,
    //                    context.TempTargetCollectionLoopingVar,
    //                    context.ReferenceTracker,
    //                    context.Mapper
    //                )
    //            );
    //        }
    //        else if( this.IsCopySourceToTempCollection )
    //        {
    //            if( context.IsTargetElementTypeBuiltIn )
    //            {
    //                return Expression.Block
    //                (
    //                    paramsExp,

    //                    Expression.Assign( tempSourceColl, tempSourceCollExp ),

    //                    SimpleCollectionLoop
    //                    (
    //                        tempSourceColl,
    //                        context.SourceCollectionElementType,
    //                        context.TargetInstance,
    //                        context.TargetCollectionElementType,
    //                        targetCollInsertionMethod,
    //                        context.SourceCollectionLoopingVar
    //                    )
    //                );
    //            }

    //            return Expression.Block
    //            (
    //                paramsExp,

    //                Expression.Assign( tempSourceColl, tempSourceCollExp ),

    //                //isMergeOrUpdate ?
    //                //    ComplexCollectionLoop
    //                //    (
    //                //        context.TargetInstance,
    //                //        context.TargetCollectionElementType,
    //                //        tempSourceColl,
    //                //        context.SourceCollectionElementType,
    //                //        tempSourceCollInsertionMethod,
    //                //        context.SourceCollectionLoopingVar,
    //                //        context.ReferenceTracker,
    //                //        context.Mapper
    //                //    ) : Expression.Empty(),

    //                //isUpdate ? GetUpdateCollectionExpression( context )
    //                //    : 
    //                ComplexCollectionLoop
    //                (
    //                    tempSourceColl,
    //                    context.SourceCollectionElementType,
    //                    context.TargetInstance,
    //                    context.TargetCollectionElementType,
    //                    targetCollInsertionMethod,
    //                    context.SourceCollectionLoopingVar,
    //                    context.ReferenceTracker,
    //                    context.Mapper
    //                )
    //            );
    //        }

    //        return Expression.Empty();
    //    }

    //    //protected virtual MethodInfo GetSourceTempCollectionInsertionMethod( CollectionMapperContext context )
    //    //{
    //    //    //utile soltanto se la source collection non ha un costruttore con parametro IEnumerable<>
    //    //    return GetTempCollectionInsertionMethod( context );
    //    //}

    //    protected virtual MethodInfo GetTargetTempCollectionInsertionMethod( CollectionMapperContext context )
    //    {
    //        return GetTempCollectionInsertionMethod( context );
    //    }

    //    private MethodInfo GetTempCollectionInsertionMethod( CollectionMapperContext context )
    //    {
    //        var collectionType = this.GetTargetTempCollectionType( context );
    //        var insertionMethod = collectionType.GetMethod( nameof( List<int>.Add ) );

    //        if( insertionMethod == null )
    //        {
    //            string msg = $@"'{collectionType}' does not provide the specified insertion method.";
    //            throw new MethodAccessException( msg );
    //        }

    //        return insertionMethod;
    //    }

    //    /// <summary>
    //    /// Get the type to use when creating a temporary collection for the source collection
    //    /// </summary>
    //    /// <param name="context"></param>
    //    /// <returns></returns>
    //    protected virtual Type GetSourceTempCollectionType( CollectionMapperContext context )
    //    {
    //        return typeof( List<> ).MakeGenericType( context.SourceCollectionElementType );
    //    }

    //    /// <summary>
    //    /// Get the type to use when creating a temporary collection for the target collection
    //    /// </summary>
    //    /// <param name="context"></param>
    //    /// <returns></returns>
    //    protected virtual Type GetTargetTempCollectionType( CollectionMapperContext context )
    //    {
    //        return typeof( List<> ).MakeGenericType( context.TargetCollectionElementType );
    //    }

    //    /// <summary>
    //    /// When set to true, indicates to use a temp collection for the target collection.
    //    /// All of the mapping work is done on the temporary collection.
    //    /// The target instance is the build from the temp collection.
    //    /// </summary>
    //    protected abstract bool IsCopySourceToTempCollection { get; }

    //    /// <summary>
    //    /// When set to true, indicates to use a temp collection for the source collection.
    //    /// All of the mapping work is done on the temporary collection.
    //    /// The source instance is the build from the temp collection.
    //    /// </summary>
    //    protected abstract bool IsCopyTargetToTempCollection { get; }
    //}

    public abstract class CollectionMappingViaTempCollection : CollectionMapper
    {
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
            var tempCollectionInsertionMethod = this.GetTempCollectionInsertionMethod( context );

            bool isUpdate = context.Options.CollectionBehavior == CollectionBehaviors.UPDATE;
            bool isReset = context.Options.CollectionBehavior == CollectionBehaviors.RESET;

            if( context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    new[] { tempCollection },

                    isReset ? GetTargetCollectionClearExpression( context ) : Expression.Empty(),

                    Expression.Assign( tempCollection, newTempCollectionExp ),

                    SimpleCollectionLoop
                    (
                        tempCollection,
                        context.SourceCollectionElementType,
                        context.TargetInstance,
                        context.TargetCollectionElementType,
                        tempCollectionInsertionMethod,
                        context.SourceCollectionLoopingVar,
                        context.Mapper,
                        context.ReferenceTracker,
                        context
                    )
                );
            }

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),

                isUpdate ? GetUpdateCollectionExpression( context ) :
                    ComplexCollectionLoop
                    (
                        tempCollection,
                        context.SourceCollectionElementType,
                        context.TargetInstance,
                        context.TargetCollectionElementType,
                        tempCollectionInsertionMethod,
                        context.SourceCollectionLoopingVar,
                        context.ReferenceTracker,
                        context.Mapper,
                        context
                    )
            );
        }

        protected virtual MethodInfo GetTempCollectionInsertionMethod( CollectionMapperContext context )
        {
            return this.GetTemporaryCollectionType( context ).GetMethod( nameof( List<int>.Add ) );
        }

        protected virtual Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( List<> ).MakeGenericType( context.SourceCollectionElementType );
        }
    }
}
