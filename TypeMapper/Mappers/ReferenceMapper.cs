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

        private static Expression<Func<ReferenceTracking, object, Type, object>> lookup =
            ( rT, sI, tT ) => refTrackingLookup( rT, sI, tT );

        private static Expression<Action<ReferenceTracking, object, Type, object>> add =
            ( rT, sI, tT, tI ) => addToTracker( rT, sI, tT, tI );

        public bool CanHandle( PropertyMapping mapping )
        {
            bool valueTypes = !mapping.SourceProperty.PropertyInfo.PropertyType.IsValueType &&
                          !mapping.TargetProperty.PropertyInfo.PropertyType.IsValueType;

            return valueTypes && !mapping.TargetProperty.IsBuiltInType &&
                !mapping.SourceProperty.IsBuiltInType && !mapping.SourceProperty.IsEnumerable;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, ObjectPair>

            var returnType = typeof( ObjectPair );
            var returnTypeConstructor = returnType.GetConstructors().First();

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var result = Expression.Variable( returnType, "result" );
            var newInstance = Expression.Variable( targetPropertyType, "newInstance" );
            var sourceArg = Expression.Variable( sourcePropertyType, "sourceArg" );

            var nullSourceValue = Expression.Default( sourcePropertyType );
            var nullTargetValue = Expression.Default( targetPropertyType );

            var body = (Expression)Expression.Block
            (
                new ParameterExpression[] { sourceArg, newInstance, result },

                //read source value
                Expression.Assign( sourceArg, mapping.SourceProperty.ValueGetter.Body.ReplaceParameter( sourceInstance ) ),

                Expression.IfThenElse
                (
                     Expression.Equal( sourceArg, nullSourceValue ),

                     Expression.Assign( newInstance, nullTargetValue ),

                     Expression.Block
                     (
                        //object lookup
                        Expression.Assign( newInstance, Expression.Convert( Expression.Invoke( lookup,
                            referenceTrack, sourceArg, Expression.Constant( targetPropertyType ) ), targetPropertyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( newInstance, nullTargetValue ),
                            Expression.Block
                            (                                
                                Expression.Assign( newInstance, Expression.New( targetPropertyType ) ),
                                
                                //cache reference
                                Expression.Invoke( add, referenceTrack, sourceArg, Expression.Constant( targetPropertyType ), newInstance ),

                                //add item to return collection
                                Expression.Assign( result, Expression.New( returnTypeConstructor, sourceArg, newInstance ) )
                            )
                        )
                    )
                ),

                mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( targetInstance, "target" )
                    .ReplaceParameter( newInstance, "value" ),

                result
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, returnType );

            return Expression.Lambda( delegateType,
                body, referenceTrack, sourceInstance, targetInstance );
        }
    }
}
