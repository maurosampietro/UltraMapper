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

        private static Expression<Func<ReferenceTracking, object, Type, object>> lookup =
             ( rT, sI, tT ) => refTrackingLookup( rT, sI, tT );

        private static Expression<Action<ReferenceTracking, object, Type, object>> add =
            ( rT, sI, tT, tI ) => addToTracker( rT, sI, tT, tI );

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

            var sourceCollection = Expression.Variable( sourceCollectionType, "sourceCollection" );
            var targetCollection = Expression.Variable( targetCollectionType, "targetCollection" );
            var nullExp = Expression.Constant( null, sourceCollectionType );

            var loopVar = Expression.Parameter( sourceElementType, "loopVar" );
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
                    Expression.Assign( targetCollection, Expression.Convert( Expression.Invoke( lookup,
                        referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ) ), targetCollectionType ) ),

                    Expression.IfThen
                    (
                        Expression.Equal( targetCollection, Expression.Constant( null, targetCollectionType ) ),

                        Expression.Block
                        (
                            Expression.Assign( targetCollection, newInstanceExp ),
                            ExpressionLoops.ForEach( sourceCollection, loopVar, Expression.Call( targetCollection, addMethod, loopVar ) ),

                            //cache new collection
                            Expression.Invoke( add, referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ), targetCollection )
                        )
                    )
                );
            }
            else
            {
                var addToRefCollectionMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var objectPairConstructor = returnElementType.GetConstructors().First();
                var newElement = Expression.Variable( targetElementType, "newElement" );

                innerBody = Expression.Block
                (
                    Expression.Assign( targetCollection, Expression.Convert( Expression.Invoke( lookup,
                        referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ) ), targetCollectionType ) ),

                    Expression.IfThen
                    (
                        Expression.Equal( targetCollection, Expression.Constant( null, targetCollectionType ) ),
                        Expression.Block
                        (
                            new[] { newElement },

                            Expression.Assign( targetCollection, newInstanceExp ),
                            ExpressionLoops.ForEach( sourceCollection, loopVar, Expression.Block
                            (
                                Expression.Assign( newElement, Expression.New( targetElementType ) ),
                                Expression.Call( targetCollection, addMethod, newElement ),

                                Expression.Call( newRefObjects, addToRefCollectionMethod,
                                    Expression.New( objectPairConstructor, loopVar, newElement ) )
                            ) ),

                            //cache new collection
                            Expression.Invoke( add, referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ), targetCollection )
                        )
                    )
                 );
            }

            var body = Expression.Block
            (
                new[] { sourceCollection, targetCollection, newRefObjects },

                Expression.Assign( newRefObjects, Expression.New( returnType ) ),
                Expression.Assign( targetCollection, Expression.Default( targetCollectionType ) ),
                Expression.Assign( sourceCollection, mapping.SourceProperty.ValueGetter.Body ),

                Expression.IfThenElse
                (
                    Expression.Equal( sourceCollection, nullExp ),

                    mapping.TargetProperty.ValueSetter.Body,

                    Expression.Block
                    (
                        innerBody,
                        mapping.TargetProperty.ValueSetter.Body
                    )
                ),

                newRefObjects
            )
            .ReplaceParameter( sourceInstance, "target" )
            .ReplaceParameter( targetInstance, "target" )
            .ReplaceParameter( targetCollection, "value" );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, referenceTrack, sourceInstance, targetInstance );
        }
    }
}
