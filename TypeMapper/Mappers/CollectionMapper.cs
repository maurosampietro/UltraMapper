using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class CollectionMapper : IObjectMapperExpression
    {
        private static Func<ReferenceTracking, object, Type, object> refTrackingLookup =
            ( referenceTracker, sourceInstance, targetType ) =>
        {
            object targetInstance;
            referenceTracker.TryGetValue( sourceInstance, targetType, out targetInstance );

            return targetInstance;
        };

        private static Action<ReferenceTracking, object, Type, object> addToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance, targetType, targetInstance );
        };

        public bool CanHandle( PropertyMapping mapping )
        {
            //the following check avoids to treat a string as a collection
            return mapping.SourceProperty.IsEnumerable &&
                !mapping.SourceProperty.IsBuiltInType;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            var returnType = typeof( List<ObjectPair> );
            var returnElementType = typeof( ObjectPair );

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourceCollectionType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetCollectionType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceElementType = sourceCollectionType.GetCollectionGenericType();
            var targetElementType = targetCollectionType.GetCollectionGenericType();

            bool isSourceElementTypeBuiltIn = sourceCollectionType.IsBuiltInType( false );
            bool isTargetElementTypeBuiltIn = targetElementType.IsBuiltInType( false );

            var addMethod = targetCollectionType.GetMethod( "Add" );

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var sourceCollection = Expression.Variable( sourceCollectionType, "sourceArg" );
            var newCollection = Expression.Variable( targetCollectionType, "newCollection" );
            var nullExp = Expression.Constant( null, sourceCollectionType );

            var loopVar = Expression.Parameter( sourceElementType, "loopVar" );

            Expression<Func<ReferenceTracking, object, Type, object>> lookup =
                ( rT, sI, tT ) => refTrackingLookup( rT, sI, tT );

            Expression<Action<ReferenceTracking, object, Type, object>> add =
                ( rT, sI, tT, tI ) => addToTracker( rT, sI, tT, tI );

            var newRefObjects = Expression.Variable( returnType, "result" );

            var newInstanceExp = Expression.New( targetCollectionType );
            if( targetCollectionType.IsCollectionOfType( typeof( List<> ) ) )
            {
                var constructorWithCapacity = targetCollectionType.GetConstructor( new Type[] { typeof( int ) } );
                var getCountMethod = targetCollectionType.GetProperty( "Count" ).GetGetMethod();

                newInstanceExp = Expression.New( constructorWithCapacity, Expression.Call( sourceCollection, getCountMethod ) );
            }


            Expression innerBody = null;
            if( isTargetElementTypeBuiltIn )
            {
                innerBody = Expression.Block
                (
                    Expression.Assign( newCollection, newInstanceExp ),
                    ExpressionLoops.ForEach( sourceCollection, loopVar, Expression.Call( newCollection, addMethod, loopVar ) )
                );
            }
            else
            {
                var addToRefCollectionMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var objectPairConstructor = returnElementType.GetConstructors().First();
                var objPair = Expression.Variable( returnElementType, "objPair" );
                var newElement = Expression.Variable( targetElementType, "newElement" );

                innerBody = Expression.Block
                (
                    new[] { newElement },

                    Expression.Assign( newCollection, newInstanceExp ),
                    ExpressionLoops.ForEach( sourceCollection, loopVar, Expression.Block
                    (
                        Expression.Assign( newElement, Expression.New( targetElementType ) ),
                        Expression.Call( newCollection, addMethod, newElement ),

                        Expression.Assign( objPair, Expression.New( objectPairConstructor, loopVar, newElement ) ),
                        Expression.Call( newRefObjects, addToRefCollectionMethod, objPair )
                    ) )
                );
            }


            var body = Expression.Block
            (
                new[] { sourceCollection, newCollection, newRefObjects },

                Expression.Assign( newRefObjects, Expression.New( returnType ) ),

                Expression.Assign( sourceCollection, Expression.Invoke(
                    mapping.SourceProperty.ValueGetterExpr, sourceInstance ) ),

                Expression.IfThenElse
                (
                    Expression.Equal( sourceCollection, nullExp ),

                    Expression.Invoke( mapping.TargetProperty.ValueSetterExpr,
                        targetInstance, Expression.Constant( null, targetCollectionType ) ),

                    Expression.Block
                    (
                        Expression.Assign( newCollection, Expression.Convert( Expression.Invoke( lookup,
                            referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ) ), targetCollectionType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( newCollection, Expression.Constant( null, targetCollectionType ) ),
                            innerBody
                        ),

                        Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance, newCollection )
                    )
                ),

               newRefObjects
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, referenceTrack, sourceInstance, targetInstance );
        }
    }

    //public class CollectionMapper : IObjectMapperExpression
    //{
    //    private static Dictionary<TypePair, Action<object, object>> _cache =
    //        new Dictionary<TypePair, Action<object, object>>();

    //    private static Dictionary<TypePair, Action<object, object>> _cache2 =
    //        new Dictionary<TypePair, Action<object, object>>();

    //    public bool CanHandle( PropertyMapping mapping )
    //    {
    //        //the following check avoids to treat a string as a collection
    //        return mapping.SourceProperty.IsEnumerable &&
    //            !mapping.SourceProperty.IsBuiltInType;
    //    }

    //    public IEnumerable<ObjectPair> Map( object source, object targetInstance,
    //        PropertyMapping mapping, IReferenceTracking referenceTracking )
    //    {
    //        var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

    //        object trackedCollection;
    //        if( referenceTracking.TryGetValue( source,
    //            targetPropertyType, out trackedCollection ) )
    //        {
    //            //mapping.TargetProperty.ValueSetter( targetInstance, trackedCollection );
    //            yield break;
    //        }

    //        //map 'the container' itself
    //        var targetCollection = mapping.TargetProperty.CollectionStrategy
    //            .GetTargetCollection<object>( targetInstance, mapping );

    //        referenceTracking.Add( source, targetPropertyType, targetCollection );
    //        //mapping.TargetProperty.ValueSetter( targetInstance, targetCollection );

    //        //map contained items
    //        Type genericType = targetPropertyType.GetCollectionGenericType();
    //        bool isBuiltInType = genericType.IsBuiltInType( false );


    //        var key = new TypePair( targetCollection.GetType(), genericType );
    //        Action<object, object> addfunc;

    //        if( !_cache.TryGetValue( key, out addfunc ) )
    //        {
    //            var destinationCollectionType = typeof( ICollection<> ).MakeGenericType( genericType );
    //            var addMethod = destinationCollectionType.GetMethod( "Add" );
    //            var instance = Expression.Parameter( typeof( object ), "instance" );
    //            var item = Expression.Parameter( typeof( object ), "item" );


    //            var loopBody = Expression.Call(
    //                    Expression.Convert( instance, targetCollection.GetType() ), addMethod,
    //                    Expression.Convert( item, genericType ) );

    //            var t = Expression.Lambda<Action<object, object>>( loopBody, instance, item );

    //            addfunc = t.Compile();
    //            _cache.Add( key, addfunc );
    //        }



    //        if( isBuiltInType )
    //        {
    //            Action<object, object> collFunc;
    //            if( !_cache2.TryGetValue( key, out collFunc ) )
    //            {
    //                var destinationCollectionType = typeof( ICollection<> ).MakeGenericType( genericType );
    //                var addMethod = destinationCollectionType.GetMethod( "Add" );
    //                var instance = Expression.Parameter( typeof( object ), "instance" );
    //                var item = Expression.Parameter( typeof( object ), "item" );

    //                var loopBody = Expression.Call(
    //                        Expression.Convert( instance, targetCollection.GetType() ), addMethod,
    //                        Expression.Convert( item, genericType ) );

    //                var t = Expression.Lambda<Action<object, object>>( loopBody, instance, item );
    //                var sourceParam = Expression.Parameter( typeof( object ), "sourceCollection" );
    //                var targetParam = Expression.Parameter( typeof( object ), "targetCollection" );

    //                var co = Expression.Variable( targetCollection.GetType(), "co" );

    //                var loopVar = Expression.Parameter( genericType, "loopVar" );
    //                var lambda = Expression.Lambda<Action<object, object>>( Expression.Block( new[] { co },
    //                   Expression.Assign( co, Expression.Convert( targetParam, targetCollection.GetType() ) ),

    //                    ForEach(
    //                    Expression.Convert( sourceParam, source.GetType() ), loopVar,

    //                        Expression.Call( co, addMethod, loopVar )
    //                ) ), sourceParam, targetParam );

    //                collFunc = lambda.Compile();
    //                _cache2.Add( key, collFunc );
    //            }

    //            collFunc( source, targetCollection );
    //            //foreach( var sourceItem in (IEnumerable)source )
    //            //    addfunc( collection, sourceItem );
    //        }
    //        else
    //        {
    //            foreach( var sourceItem in (IEnumerable)source )
    //            {
    //                object targetItem;
    //                if( !referenceTracking.TryGetValue( sourceItem,
    //                    genericType, out targetItem ) )
    //                {
    //                    targetItem = Activator.CreateInstance( genericType );

    //                    //track these references BEFORE recursion to avoid infinite loops and stackoverflow
    //                    referenceTracking.Add( sourceItem, genericType, targetItem );
    //                    yield return new ObjectPair( sourceItem, targetItem );
    //                }

    //                addfunc( targetCollection, targetItem );
    //            }
    //        }
    //    }
    //}
}
