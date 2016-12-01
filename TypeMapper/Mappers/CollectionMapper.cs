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
    public class CollectionMapper : IObjectMapper
    {
        private static Dictionary<TypePair, Action<object, object>> _cache =
            new Dictionary<TypePair, Action<object, object>>();

        private static Dictionary<TypePair, Action<object, object>> _cache2 =
            new Dictionary<TypePair, Action<object, object>>();

        public bool CanHandle( PropertyMapping mapping )
        {
            //the following check avoids to treat a string as a collection
            return mapping.SourceProperty.IsEnumerable &&
                !mapping.SourceProperty.IsBuiltInType;
        }

        public IEnumerable<ObjectPair> Map( object source, object targetInstance,
            PropertyMapping mapping, IReferenceTracking referenceTracking )
        {
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            object trackedCollection;
            if( referenceTracking.TryGetValue( source,
                targetPropertyType, out trackedCollection ) )
            {
                //mapping.TargetProperty.ValueSetter( targetInstance, trackedCollection );
                yield break;
            }

            //map 'the container' itself
            var targetCollection = mapping.TargetProperty.CollectionStrategy
                .GetTargetCollection<object>( targetInstance, mapping );

            referenceTracking.Add( source, targetPropertyType, targetCollection );
            //mapping.TargetProperty.ValueSetter( targetInstance, targetCollection );

            //map contained items
            Type genericType = targetPropertyType.GetCollectionGenericType();
            bool isBuiltInType = genericType.IsBuiltInType( false );


            var key = new TypePair( targetCollection.GetType(), genericType );
            Action<object, object> addfunc;

            if( !_cache.TryGetValue( key, out addfunc ) )
            {
                var destinationCollectionType = typeof( ICollection<> ).MakeGenericType( genericType );
                var addMethod = destinationCollectionType.GetMethod( "Add" );
                var instance = Expression.Parameter( typeof( object ), "instance" );
                var item = Expression.Parameter( typeof( object ), "item" );


                var loopBody = Expression.Call(
                        Expression.Convert( instance, targetCollection.GetType() ), addMethod,
                        Expression.Convert( item, genericType ) );

                var t = Expression.Lambda<Action<object, object>>( loopBody, instance, item );

                addfunc = t.Compile();
                _cache.Add( key, addfunc );
            }



            if( isBuiltInType )
            {
                Action<object, object> collFunc;
                if( !_cache2.TryGetValue( key, out collFunc ) )
                {
                    var destinationCollectionType = typeof( ICollection<> ).MakeGenericType( genericType );
                    var addMethod = destinationCollectionType.GetMethod( "Add" );
                    var instance = Expression.Parameter( typeof( object ), "instance" );
                    var item = Expression.Parameter( typeof( object ), "item" );

                    var loopBody = Expression.Call(
                            Expression.Convert( instance, targetCollection.GetType() ), addMethod,
                            Expression.Convert( item, genericType ) );

                    var t = Expression.Lambda<Action<object, object>>( loopBody, instance, item );
                    var sourceParam = Expression.Parameter( typeof( object ), "sourceCollection" );
                    var targetParam = Expression.Parameter( typeof( object ), "targetCollection" );

                    var co = Expression.Variable( targetCollection.GetType(), "co" );

                    var loopVar = Expression.Parameter( genericType, "loopVar" );
                    var lambda = Expression.Lambda<Action<object, object>>( Expression.Block( new[] { co },
                       Expression.Assign( co, Expression.Convert( targetParam, targetCollection.GetType() ) ),

                        ForEach(
                        Expression.Convert( sourceParam, source.GetType() ), loopVar,

                            Expression.Call( co, addMethod, loopVar )
                    ) ), sourceParam, targetParam );

                    collFunc = lambda.Compile();
                    _cache2.Add( key, collFunc );
                }

                collFunc( source, targetCollection );
                //foreach( var sourceItem in (IEnumerable)source )
                //    addfunc( collection, sourceItem );
            }
            else
            {
                foreach( var sourceItem in (IEnumerable)source )
                {
                    object targetItem;
                    if( !referenceTracking.TryGetValue( sourceItem,
                        genericType, out targetItem ) )
                    {
                        targetItem = Activator.CreateInstance( genericType );

                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                        referenceTracking.Add( sourceItem, genericType, targetItem );
                        yield return new ObjectPair( sourceItem, targetItem );
                    }

                    addfunc( targetCollection, targetItem );
                }
            }
        }

        public static Expression ForEach( Expression collection, ParameterExpression loopVar, Expression loopContent, Expression outLoopInitializations = null )
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof( IEnumerable<> ).MakeGenericType( elementType );
            var enumeratorType = typeof( IEnumerator<> ).MakeGenericType( elementType );

            var enumeratorVar = Expression.Variable( enumeratorType, "enumerator" );
            var getEnumeratorCall = Expression.Call( collection, enumerableType.GetMethod( "GetEnumerator" ) );
            var enumeratorAssign = Expression.Assign( enumeratorVar, getEnumeratorCall );

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call( enumeratorVar, typeof( IEnumerator ).GetMethod( "MoveNext" ) );

            var breakLabel = Expression.Label( "LoopBreak" );

            if( outLoopInitializations == null )
                outLoopInitializations = Expression.Empty();

            var loop = Expression.Block( new[] { enumeratorVar },
                enumeratorAssign, outLoopInitializations,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal( moveNextCall, Expression.Constant( true ) ),
                        Expression.Block( new[] { loopVar },
                            Expression.Assign( loopVar, Expression.Property( enumeratorVar, "Current" ) ),
                            loopContent
                        ),
                        Expression.Break( breakLabel )
                    ),
                breakLabel )
            );

            return loop;
        }
    }
}
