using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Configuration;
using UltraMapper.Internals;

namespace UltraMapper.Mappers
{
    public class DictionaryMapper : CollectionMapper
    {
        public DictionaryMapper( MapperConfiguration configuration )
             : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            bool sourceIsDictionary = typeof( IDictionary ).IsAssignableFrom( source );
            bool targetIsDictionary = typeof( IDictionary ).IsAssignableFrom( target );

            return sourceIsDictionary || targetIsDictionary;
        }

        protected override ReferenceMapperContext GetMapperContext( Type source, Type target )
        {
            return new DictionaryMapperContext( source, target );
        }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as DictionaryMapperContext;

            var addMethod = base.GetTargetCollectionInsertionMethod( context );

            var keyExpression = this.GetKeyOrValueExpression( context,
                context.SourceCollectionElementKey, context.TargetCollectionElementKey );

            var valueExpression = this.GetKeyOrValueExpression( context,
                context.SourceCollectionElementValue, context.TargetCollectionElementValue );

            return Expression.Block
            (
                new[] { context.SourceCollectionElementKey, context.SourceCollectionElementValue,
                    context.TargetCollectionElementKey, context.TargetCollectionElementValue },

                ExpressionLoops.ForEach( context.SourceInstance,
                    context.SourceCollectionLoopingVar, Expression.Block
                (
                    Expression.Assign( context.SourceCollectionElementKey,
                        Expression.Property( context.SourceCollectionLoopingVar, nameof( DictionaryEntry.Key ) ) ),

                    Expression.Assign( context.SourceCollectionElementValue,
                        Expression.Property( context.SourceCollectionLoopingVar, nameof( DictionaryEntry.Value ) ) ),

                    keyExpression,
                    valueExpression,

                    Expression.Call( context.TargetInstance, addMethod,
                        context.TargetCollectionElementKey, context.TargetCollectionElementValue )
                ) )
            );
        }

        protected virtual Expression GetKeyOrValueExpression( DictionaryMapperContext context,
            ParameterExpression sourceParam, ParameterExpression targetParam )
        {
            var itemMapping = MapperConfiguration[ sourceParam.Type,
                targetParam.Type ].MappingExpression;

            if( sourceParam.Type.IsBuiltInType( false ) && 
                targetParam.Type.IsBuiltInType( false ) )
            {
                var itemMappingExp = itemMapping.Body.ReplaceParameter(
                    sourceParam, itemMapping.Parameters[ 0 ].Name );

                return Expression.Assign( targetParam, itemMappingExp );
            }

            return base.LookUpBlock( itemMapping, context, 
                context.ReferenceTracker, sourceParam, targetParam );
        }
    }
}
