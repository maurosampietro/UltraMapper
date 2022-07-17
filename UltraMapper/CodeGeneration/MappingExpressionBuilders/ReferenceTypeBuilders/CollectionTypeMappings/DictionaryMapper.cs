using System;
using System.Collections;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class DictionaryMapper : CollectionMapper
    {
        public DictionaryMapper( Configuration configuration )
             : base( configuration ) { }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            bool sourceIsDictionary = typeof( IDictionary ).IsAssignableFrom( source.EntryType );
            bool targetIsDictionary = typeof( IDictionary ).IsAssignableFrom( target.EntryType );

            return sourceIsDictionary || targetIsDictionary;
        }

        protected override ReferenceMapperContext GetMapperContext( Mapping mapping )
        {
            var source = mapping.Source.EntryType;
            var target = mapping.Target.EntryType;

            return new DictionaryMapperContext(mapping );
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
                new[] { context.Mapper, context.SourceCollectionElementKey, context.SourceCollectionElementValue,
                    context.TargetCollectionElementKey, context.TargetCollectionElementValue },

                Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),

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
            if( (sourceParam.Type.IsBuiltIn( false ) && targetParam.Type.IsBuiltIn( false )) ||
                (!sourceParam.Type.IsClass || !targetParam.Type.IsClass) )
            {
                var itemMapping = MapperConfiguration[ sourceParam.Type,
                    targetParam.Type ].MappingExpression;

                var itemMappingExp = itemMapping.Body.ReplaceParameter(
                    sourceParam, "sourceInstance" );

                return Expression.Assign( targetParam, itemMappingExp );
            }

            return base.LookUpBlock( sourceParam, targetParam,
                context.ReferenceTracker, context.Mapper );
        }
    }
}
