using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    /*NOTES:
     * 
     *- Collections that do not implement ICollection<T> must specify which method
     *to use to 'Add' an item or must have a constructor that takes as param a IEnumerable.
     * 
     *- Stack<T> an other LIFO collections require the list to be read in reverse  
     * while to preserve order and have a specular clone. This is done with Stack<T> by
     * creating the list two times: 'new Stack( new Stack( sourceCollection ) )'
     * 
     *- SortedSet<T> need immediate recursion
     *in order for the item to be added to the collection if T is a 
     *complex type that implements IComparable<T> 
     * 
     * - HashSet<T> need immediate recursion
     *in order for the item to be added to the collection if T is a 
     *complex type that overrides GetHashCode and Equals
     * 
     * 
     */

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

        public virtual bool CanHandle( PropertyMapping mapping )
        {
            //the following check avoids to treat a string as a collection
            return mapping.SourceProperty.IsEnumerable &&
                !mapping.SourceProperty.IsBuiltInType;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            var context = new CollectionMapperContext( mapping );
            var addMethod = GetTargetCollectionAddMethod( context );
            Expression innerBody = null;

            if( context.IsTargetElementTypeBuiltIn )
            {
                Expression elementsCopy =
                    ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar,
                        Expression.Call( context.TargetCollection, addMethod, context.SourceLoopingVar ) );

                var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
                if( constructorInfo != null )
                    elementsCopy = Expression.Assign( context.TargetCollection,
                        GetTargetCollectionConstructorFromCollectionExpression( context, context.SourceCollection ) );

                innerBody = Expression.Block
                (
                    Expression.Assign( context.TargetCollection, Expression.Convert( Expression.Invoke( lookup,
                       context.ReferenceTrack, context.SourceCollection, Expression.Constant( context.TargetCollectionType ) ), context.TargetCollectionType ) ),

                    Expression.IfThen
                    (
                        Expression.Equal( context.TargetCollection, Expression.Constant( null, context.TargetCollectionType ) ),

                        Expression.Block
                        (
                            elementsCopy,

                            //cache new collection
                            Expression.Invoke( add, context.ReferenceTrack, context.SourceCollection,
                                Expression.Constant( context.TargetCollectionType ), context.TargetCollection )
                        )
                    )
                );
            }
            else
            {
                var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var addRangeToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
                var objectPairConstructor = context.ReturnElementType.GetConstructors().First();
                var newElement = Expression.Variable( context.TargetElementType, "newElement" );

                var newInstanceExp = Expression.New( context.TargetCollectionType );
                if( context.TargetCollectionType.IsCollectionOfType( typeof( ICollection<> ) ) )
                {
                    var constructorWithCapacity = context.TargetCollectionType.GetConstructor( new Type[] { typeof( int ) } );
                    var getCountMethod = context.TargetCollectionType.GetProperty( "Count" ).GetGetMethod();

                    newInstanceExp = Expression.New( constructorWithCapacity, Expression.Call( context.SourceCollection, getCountMethod ) );
                }

                //if collection needs immediate recursion on items (before adding the item to the collection itself)
                bool needsImmediateRecursionOnItem = context.TargetCollectionType.ImplementsInterface( typeof( ISet<> )
                    .MakeGenericType( context.TargetElementType ) );

                if( needsImmediateRecursionOnItem )
                {
                    var itemMapping = mapping.TypeMapping.GlobalConfiguration.Configurator[
                        context.SourceElementType, context.TargetElementType ].MappingExpression;

                    innerBody = Expression.Block
                    (
                        Expression.Assign( context.TargetCollection, Expression.Convert( Expression.Invoke( lookup,
                            context.ReferenceTrack, context.SourceCollection, Expression.Constant( context.TargetCollectionType ) ), context.TargetCollectionType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetCollection, Expression.Constant( null, context.TargetCollectionType ) ),
                            Expression.Block
                            (
                                new[] { newElement },

                                Expression.Assign( context.TargetCollection, newInstanceExp ),
                                ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar, Expression.Block
                                (
                                    Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                                    Expression.Call( context.NewRefObjects, addRangeToRefCollectionMethod, Expression.Invoke(
                                        itemMapping, context.ReferenceTrack, context.SourceLoopingVar, newElement ) ),

                                    Expression.Call( context.TargetCollection, addMethod, newElement )
                                ) ),

                                //cache new collection
                                Expression.Invoke( add, context.ReferenceTrack, context.SourceCollection,
                                    Expression.Constant( context.TargetCollectionType ), context.TargetCollection )
                            )
                        )
                     );
                }
                else
                {
                    if( this.AvoidAddCalls( context ) )
                    {
                        var tempCollectionType = this.GetTempCollectionType( context );
                        if( !tempCollectionType.ImplementsInterface( typeof( ICollection<> ) ) )
                            throw new Exception( $"A temporary collection must be a type implementing ICollection<TargetElementType>" );
                        
                        var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );
                        var tempCollectionAddMethod = tempCollectionType.GetMethod( "Add" );

                        var constructorWithCapacity = tempCollectionType.GetConstructor( new Type[] { typeof( int ) } );
                        var getCountMethod = context.SourceCollectionType.GetProperty( "Count" ).GetGetMethod();

                        var newTempCollectionExp = Expression.New( constructorWithCapacity,
                            Expression.Call( context.SourceCollection, getCountMethod ) );

                        var targetCollectionConstructor = GetTargetCollectionConstructorFromCollectionExpression( context,tempCollection );

                        innerBody = Expression.Block
                        (
                            Expression.Assign( context.TargetCollection, Expression.Convert( Expression.Invoke( lookup,
                                context.ReferenceTrack, context.SourceCollection, Expression.Constant( context.TargetCollectionType ) ), context.TargetCollectionType ) ),

                            Expression.IfThen
                            (
                                Expression.Equal( context.TargetCollection, Expression.Constant( null, context.TargetCollectionType ) ),
                                Expression.Block
                                (
                                    new[] { newElement, tempCollection },

                                    Expression.Assign( tempCollection, newTempCollectionExp ),
                                    ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar, Expression.Block
                                    (
                                        Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                                        Expression.Call( tempCollection, tempCollectionAddMethod, newElement ),

                                        Expression.Call( context.NewRefObjects, addToRefCollectionMethod,
                                            Expression.New( objectPairConstructor, context.SourceLoopingVar, newElement ) )
                                    ) ),

                                    Expression.Assign( context.TargetCollection, targetCollectionConstructor ),

                                    //cache new collection
                                    Expression.Invoke( add, context.ReferenceTrack, context.SourceCollection,
                                        Expression.Constant( context.TargetCollectionType ), context.TargetCollection )
                                )
                            )
                         );
                    }
                    else
                    {
                        innerBody = Expression.Block
                        (
                            Expression.Assign( context.TargetCollection, Expression.Convert( Expression.Invoke( lookup,
                                context.ReferenceTrack, context.SourceCollection, Expression.Constant( context.TargetCollectionType ) ), context.TargetCollectionType ) ),

                            Expression.IfThen
                            (
                                Expression.Equal( context.TargetCollection, Expression.Constant( null, context.TargetCollectionType ) ),
                                Expression.Block
                                (
                                    new[] { newElement },

                                    Expression.Assign( context.TargetCollection, newInstanceExp ),
                                    ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar, Expression.Block
                                    (
                                        Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                                        Expression.Call( context.TargetCollection, addMethod, newElement ),

                                        Expression.Call( context.NewRefObjects, addToRefCollectionMethod,
                                            Expression.New( objectPairConstructor, context.SourceLoopingVar, newElement ) )
                                    ) ),

                                    //cache new collection
                                    Expression.Invoke( add, context.ReferenceTrack, context.SourceCollection,
                                        Expression.Constant( context.TargetCollectionType ), context.TargetCollection )
                                )
                            )
                         );
                    }
                }
            }

            var body = Expression.Block
            (
                new[] { context.SourceCollection, context.TargetCollection, context.NewRefObjects },

                Expression.Assign( context.NewRefObjects, Expression.New( context.ReturnType ) ),
                Expression.Assign( context.TargetCollection, Expression.Constant( null, context.TargetCollectionType ) ),
                Expression.Assign( context.SourceCollection, mapping.SourceProperty.ValueGetter.Body
                    .ReplaceParameter( context.SourceInstance, "target" ) ),

                Expression.IfThenElse
                (
                    Expression.Equal( context.SourceCollection, context.SourceNull ),

                    mapping.TargetProperty.ValueSetter.Body
                        .ReplaceParameter( context.TargetInstance, "target" ),

                    Expression.Block
                    (
                        innerBody,
                        mapping.TargetProperty.ValueSetter.Body
                            .ReplaceParameter( context.TargetInstance, "target" )
                    )
                ),

                context.NewRefObjects
            )
            .ReplaceParameter( context.TargetCollection, "value" );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceType, context.TargetType, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }

        protected virtual Type GetTempCollectionType( CollectionMapperContext context )
        {
            return typeof( List<> ).MakeGenericType( context.TargetElementType );
        }

        protected virtual ConstructorInfo GetTargetCollectionConstructorFromCollection( CollectionMapperContext context )
        {
            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.TargetElementType ) };

            return context.TargetCollectionType.GetConstructor( paramType );
        }

        protected virtual Expression GetTargetCollectionConstructorFromCollectionExpression( 
            CollectionMapperContext context, ParameterExpression initCollection )
        {
            var constructor = GetTargetCollectionConstructorFromCollection( context );
            return Expression.New( constructor, initCollection );
        }

        /// <summary>
        /// Return the method that allows to add items to the target collection.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetCollectionType.GetMethod( "Add" );
        }

        protected virtual bool AvoidAddCalls( CollectionMapperContext context )
        {
            return false;
        }
    }

    public class CollectionMapperContext
    {
        public Type SourceCollectionType;
        public Type SourceElementType;
        public Type ReturnType { get; set; }
        public Type ReturnElementType { get; set; }
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
        public Type TargetCollectionType { get; set; }
        public Type TargetElementType { get; set; }

        public bool IsSourceElementTypeBuiltIn { get; set; }
        public bool IsTargetElementTypeBuiltIn { get; set; }

        public ParameterExpression ReferenceTrack { get; set; }
        public ParameterExpression NewRefObjects { get; set; }
        public ParameterExpression SourceInstance { get; set; }
        public ParameterExpression TargetInstance { get; set; }
        public ParameterExpression SourceCollection { get; set; }
        public ParameterExpression TargetCollection { get; set; }
        public ParameterExpression SourceLoopingVar { get; set; }
        public ConstantExpression SourceNull { get; set; }

        public CollectionMapperContext( PropertyMapping mapping )
        {
            ReturnType = typeof( List<ObjectPair> );
            ReturnElementType = typeof( ObjectPair );

            SourceType = mapping.SourceProperty.PropertyInfo.ReflectedType;
            TargetType = mapping.TargetProperty.PropertyInfo.ReflectedType;

            SourceCollectionType = mapping.SourceProperty.PropertyInfo.PropertyType;
            TargetCollectionType = mapping.TargetProperty.PropertyInfo.PropertyType;

            SourceElementType = SourceCollectionType.GetCollectionGenericType();
            TargetElementType = TargetCollectionType.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceCollectionType.IsBuiltInType( false );
            IsTargetElementTypeBuiltIn = TargetElementType.IsBuiltInType( false );

            SourceCollection = Expression.Variable( SourceCollectionType, "sourceCollection" );
            TargetCollection = Expression.Variable( TargetCollectionType, "targetCollection" );
            SourceNull = Expression.Constant( null, SourceCollectionType );

            SourceInstance = Expression.Parameter( SourceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceLoopingVar = Expression.Parameter( SourceElementType, "loopVar" );
            NewRefObjects = Expression.Variable( ReturnType, "result" );
        }
    }
}

