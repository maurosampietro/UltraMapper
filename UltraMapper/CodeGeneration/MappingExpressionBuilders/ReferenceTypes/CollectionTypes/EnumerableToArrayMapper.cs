using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Internals.ExtensionMethods;

namespace UltraMapper.MappingExpressionBuilders
{
    /// <summary>
    /// Unmaterialized enumerables resolve internally to the Iterator class and its subclasses.
    /// Unmaterialized enumerables must not read Count property to map on array.
    /// This mapper uses a temp List and then performs a .ToArray()
    /// </summary>
    public class EnumerableToArrayMapper : CollectionMapper
    {
        public override bool CanHandle( Mapping mapping )
        {
            var target = mapping.Target;
            return base.CanHandle( mapping ) && target.EntryType.IsArray;
        }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            var targetTempType = typeof( List<> ).MakeGenericType( context.TargetCollectionElementType );
            var tempColl = Expression.Parameter( targetTempType, "tempColl" );

            var tempMapping = context.MapperConfiguration[ context.SourceInstance.Type, targetTempType ]
                .MappingExpression;

            var ctorParam = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.TargetCollectionElementType ) };

            var ctor = typeof( List<> )
                .MakeGenericType( context.TargetCollectionElementType )
                .GetConstructor( ctorParam );

            var toArrayMethod = typeof( List<> )
                .MakeGenericType( context.TargetCollectionElementType )
                .GetMethod( nameof( List<int>.ToArray ) );

            return Expression.Block
            (
                new[] { tempColl },

                Expression.Assign( tempColl, Expression.New( tempColl.Type ) ),
                Expression.Invoke( tempMapping, context.ReferenceTracker, context.SourceInstance, tempColl ),
                Expression.Assign( contextObj.TargetInstance, Expression.Call( tempColl, toArrayMethod ) )
            );
        }
    }
}
