using System;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.ReferenceTracking
{
    public static class ReferenceTrackingExpression
    {
        private static readonly Func<ReferenceTracker, object, Type, object> _referenceLookup =
            ( referenceTracker, sourceInstance, targetType ) =>
        {
            referenceTracker.TryGetValue( sourceInstance,
                targetType, out object targetInstance );

            return targetInstance;
        };

        private static readonly Action<ReferenceTracker, object, Type, object> _addReferenceToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance,
                targetType, targetInstance );
        };

        public static Expression GetMappingExpression(
            ParameterExpression referenceTracker,
            ParameterExpression sourceMember,
            Expression targetMember,
            Expression memberAssignment,
            ParameterExpression mapperParam,
            Mapper mapper,
            IMapping mapping,
            Expression mappingConstExp,
            bool redirectMappingToRuntime = true )
        {
            var refLookupExp = Expression.Call
            (
                Expression.Constant( _referenceLookup.Target ),
                _referenceLookup.Method,
                referenceTracker,
                sourceMember,
                Expression.Constant( targetMember.Type )
            );

            var addRefToTrackerExp = Expression.Call
            (
                Expression.Constant( _addReferenceToTracker.Target ),
                _addReferenceToTracker.Method,
                referenceTracker,
                sourceMember,
                Expression.Constant( targetMember.Type ),
                targetMember
            );

            var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                .MakeGenericMethod( sourceMember.Type, targetMember.Type );

            var trackedReference = Expression.Parameter( targetMember.Type, "trackedReference" );

            var sourceNullConstant = Expression.Constant( null, sourceMember.Type );
            var targetNullConstant = Expression.Constant( null, targetMember.Type );

            /* SOURCE (NULL) -> TARGET = NULL
            * 
            * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
            * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
            * 
            * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NULL) = ASSIGN NEW OBJECT 
            * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
            */

            ParameterExpression[] blockParameters = new ParameterExpression[ 0 ];

            if( mapper != null && mapper.Config.IsReferenceTrackingEnabled )
                blockParameters = new ParameterExpression[] { mapperParam, trackedReference };

            else if( mapper == null ) //mapper param could be defined at higher level
                blockParameters = new ParameterExpression[] { trackedReference };

            return Expression.Block
            (
                blockParameters,

                mapper == null ? (Expression)Expression.Empty() :
                    Expression.Assign( mapperParam, Expression.Constant( mapper ) ),

                Expression.IfThenElse
                (
                     Expression.Equal( sourceMember, sourceNullConstant ),
                     Expression.Assign( targetMember, targetNullConstant ),
                     Expression.Block
                     (
                        Expression.Assign( trackedReference,
                            Expression.Convert( refLookupExp, targetMember.Type ) ),

                        Expression.IfThenElse
                        (
                            Expression.NotEqual( trackedReference, targetNullConstant ),
                            Expression.Assign( targetMember, trackedReference ),
                            Expression.Block
                            (
                                memberAssignment,
                                addRefToTrackerExp,

                                redirectMappingToRuntime ? Expression.Call( mapperParam, mapMethod, sourceMember,
                                    targetMember, referenceTracker, mappingConstExp ) :
                                    Expression.Invoke( mapping.MappingExpression, referenceTracker, sourceMember, targetMember )              
                            )
                        )
                    )
                )
            );
        }
    }
}
