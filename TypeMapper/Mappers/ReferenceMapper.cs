using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : IObjectMapperExpression
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
            bool valueTypes = !mapping.SourceProperty.PropertyInfo.PropertyType.IsValueType &&
                          !mapping.TargetProperty.PropertyInfo.PropertyType.IsValueType;

            return valueTypes && !mapping.TargetProperty.IsBuiltInType &&
                !mapping.SourceProperty.IsBuiltInType && !mapping.SourceProperty.IsEnumerable;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            var returnType = typeof( List<ObjectPair> );
            var returnElementConstructor = typeof( ObjectPair ).GetConstructors().First();
            var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            Expression<Func<ReferenceTracking, object, Type, object>> lookup =
                ( rT, sI, tT ) => refTrackingLookup( rT, sI, tT );

            Expression<Action<ReferenceTracking, object, Type, object>> add =
                ( rT, sI, tT, tI ) => addToTracker( rT, sI, tT, tI );

            var result = Expression.Variable( returnType, "result" );
            var newInstance = Expression.Variable( targetPropertyType, "newInstance" );
            var sourceArg = Expression.Variable( sourcePropertyType, "sourceArg" );

            var nullExp = Expression.Constant( null, sourcePropertyType );

            var body = Expression.Block
            (
                new ParameterExpression[] { sourceArg, newInstance, result },

                //initialize object-pairs return collection
                Expression.Assign( result, Expression.New( returnType ) ),

                //read source value
                Expression.Assign( sourceArg, Expression.Invoke(
                    mapping.SourceProperty.ValueGetterExpr, sourceInstance ) ),

                Expression.IfThenElse
                (
                     Expression.Equal( sourceArg, nullExp ),

                     Expression.Invoke( mapping.TargetProperty.ValueSetterExpr,
                        targetInstance, Expression.Constant( null, targetPropertyType ) ),

                     Expression.Block
                     (
                        //object lookup
                        Expression.Assign( newInstance, Expression.Convert( Expression.Invoke( lookup,
                            referenceTrack, sourceArg, Expression.Constant( targetPropertyType ) ), targetPropertyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( newInstance, Expression.Constant( null, targetPropertyType ) ),
                            Expression.Block
                            (
                                Expression.Assign( newInstance, Expression.New( targetPropertyType ) ),
                                Expression.Invoke( add, referenceTrack, sourceArg, Expression.Constant( targetPropertyType ), newInstance ),

                                //add item to return collection
                                Expression.Call( result, addMethod, Expression.New( returnElementConstructor, sourceArg, newInstance ) )
                            )
                        ),

                        Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance, newInstance )
                    )
                ),

                result
           );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, returnType );

            return Expression.Lambda( delegateType,
                body, referenceTrack, sourceInstance, targetInstance );
        }
    }
}
