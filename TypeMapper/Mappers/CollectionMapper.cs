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

        //protected virtual Expression GetComplexTypeInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        //{

        //}

        protected virtual Expression GetSimpleTypeInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            var addMethod = GetTargetCollectionAddMethod( context );

            var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
            if( constructorInfo == null )
            {
                Expression loopBody = Expression.Call( context.TargetCollection,
                    addMethod, context.SourceLoopingVar );

                return ExpressionLoops.ForEach( context.SourceCollection,
                    context.SourceLoopingVar, loopBody );
            }

            var constructor = GetTargetCollectionConstructorFromCollection( context );
            var targetCollectionConstructor = Expression.New( constructor, context.SourceCollection );

            return Expression.Assign( context.TargetCollection, targetCollectionConstructor );
        }

        protected virtual Expression GetComplexTypeInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            /*
             * By default try to retrieve the item insertion method of the collection.
             * The exact name of the method can be overridden so that, for example, 
             * on Queue you search for 'Enqueue'. The default method name searched is 'Add'.
             * 
             * If the item insertion method does not exist, try to retrieve a constructor
             * which takes as its only parameter 'IEnumerable<T>'. If this constructor
             * exists a temporary List<T> is created and then passed to the constructor.
             * 
             * If neither the item insertion method nor the above constructor exist
             * an exception is thrown
             */

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var addRangeToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();
            var newElement = Expression.Variable( context.TargetElementType, "newElement" );

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod != null )
            {
                var newInstanceExp = Expression.New( context.TargetCollectionType );
                if( context.TargetCollectionType.ImplementsInterface( typeof( ICollection<> ) ) )
                {
                    var constructorWithCapacity = context.TargetCollectionType.GetConstructor( new Type[] { typeof( int ) } );
                    if( constructorWithCapacity != null )
                    {
                        var getCountMethod = context.TargetCollectionType.GetProperty( "Count" ).GetGetMethod();
                        newInstanceExp = Expression.New( constructorWithCapacity, Expression.Call( context.SourceCollection, getCountMethod ) );
                    }
                }

                return Expression.Block
                (
                    new[] { newElement },

                    Expression.Assign( context.TargetCollection, newInstanceExp ),
                    ExpressionLoops.ForEach( context.SourceCollection, context.SourceLoopingVar, Expression.Block
                    (
                        Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                        Expression.Call( context.TargetCollection, addMethod, newElement ),

                        Expression.Call( context.NewRefObjects, addToRefCollectionMethod,
                            Expression.New( objectPairConstructor, context.SourceLoopingVar, newElement ) )
                    ) )
                );
            }

            //Look for the constructor which takes an initial collection as parameter
            var constructor = GetTargetCollectionConstructorFromCollection( context );
            if( constructor == null )
            {
                string msg = $@"'{nameof( context.TargetCollectionType )}' does not provide an 'Add' method or a constructor taking as parameter IEnumerable<T>. " +
                    "Please override {nameof( GetTargetCollectionAddMethod )} to provide the item insertion method.";

                throw new Exception( msg );
            }

            var tempCollectionType = typeof( List<> ).MakeGenericType( context.TargetElementType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );
            var tempCollectionAddMethod = tempCollectionType.GetMethod( "Add" );

            var tempCtorWithCapacity = tempCollectionType.GetConstructor( new Type[] { typeof( int ) } );
            var tempCollectionCountMethod = context.SourceCollectionType.GetProperty( "Count" ).GetGetMethod();

            var newTempCollectionExp = Expression.New( tempCtorWithCapacity,
                Expression.Call( context.SourceCollection, tempCollectionCountMethod ) );

            return Expression.Block
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

                Expression.Assign( context.TargetCollection, Expression.New( constructor, tempCollection ) )
            );
        }

        protected virtual Expression GetInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            if( context.IsTargetElementTypeBuiltIn )
                return GetSimpleTypeInnerBody( mapping, context );

            return GetComplexTypeInnerBody( mapping, context );
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            var context = new CollectionMapperContext( mapping );
            Expression innerBody = GetInnerBody( mapping, context );

            var lookupBody = Expression.Block
            (
                Expression.Assign( context.TargetCollection, Expression.Convert(
                    Expression.Invoke( lookup, context.ReferenceTrack, context.SourceCollection,
                    Expression.Constant( context.TargetCollectionType ) ), context.TargetCollectionType ) ),

                Expression.IfThen
                (
                    Expression.Equal( context.TargetCollection, context.TargetCollectionNull ),
                    Expression.Block
                    (
                        innerBody,

                        //cache new collection
                        Expression.Invoke( add, context.ReferenceTrack, context.SourceCollection,
                            Expression.Constant( context.TargetCollectionType ), context.TargetCollection )
                    )
                )
            );

            var body = Expression.Block
            (
                new[] { context.SourceCollection, context.TargetCollection, context.NewRefObjects },

                Expression.Assign( context.NewRefObjects, Expression.New( context.ReturnType ) ),
                Expression.Assign( context.TargetCollection, context.TargetCollectionNull ),
                Expression.Assign( context.SourceCollection, mapping.SourceProperty.ValueGetter.Body
                    .ReplaceParameter( context.SourceInstance, "target" ) ),

                Expression.IfThenElse
                (
                    Expression.Equal( context.SourceCollection, context.SourceCollectionNull ),

                    mapping.TargetProperty.ValueSetter.Body
                        .ReplaceParameter( context.TargetInstance, "target" ),

                    Expression.Block
                    (
                        lookupBody,
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

        protected virtual ConstructorInfo GetTargetCollectionConstructorFromCollection( CollectionMapperContext context )
        {
            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.TargetElementType ) };

            return context.TargetCollectionType.GetConstructor( paramType );
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
        public ConstantExpression SourceCollectionNull { get; set; }
        public Expression TargetCollectionNull { get; internal set; }

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

            SourceCollectionNull = Expression.Constant( null, SourceCollectionType );
            TargetCollectionNull = Expression.Constant( null, TargetCollectionType );

            SourceInstance = Expression.Parameter( SourceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceLoopingVar = Expression.Parameter( SourceElementType, "loopVar" );
            NewRefObjects = Expression.Variable( ReturnType, "result" );
        }
    }
}

