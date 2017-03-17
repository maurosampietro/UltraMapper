using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
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

        protected override object GetMapperContext( MemberMapping mapping )
        {
            return new DictionaryMapperContext( mapping );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as DictionaryMapperContext;

            var addMethod = base.GetTargetCollectionAddMethod( context );

            var keyExpression = this.GetKeyOrValueExpression( context,
                context.SourceCollectionElementKey, context.TargetCollectionElementKey );

            var valueExpression = this.GetKeyOrValueExpression( context,
                context.SourceCollectionElementValue, context.TargetCollectionElementValue );

            return Expression.Block
            (
                new[] { context.TargetCollectionElementKey, context.TargetCollectionElementValue },

                Expression.Assign( context.TargetMember,
                    Expression.New( context.TargetMember.Type ) ),

                ExpressionLoops.ForEach( context.SourceMember,
                    context.SourceCollectionLoopingVar, Expression.Block
                (
                    keyExpression,
                    valueExpression,

                    Expression.Call( context.TargetMember,
                        addMethod, context.TargetCollectionElementKey, context.TargetCollectionElementValue )
                ) )
            );
        }

        protected virtual Expression GetKeyOrValueExpression( DictionaryMapperContext context,
            MemberExpression sourceParam, ParameterExpression targetParam )
        {
            var typeMapping = MapperConfiguration[
                    sourceParam.Type, targetParam.Type ];

            var convert = typeMapping.MappingExpression;

            if( sourceParam.Type.IsBuiltInType( false ) && targetParam.Type.IsBuiltInType( false ) )
            {
                if( sourceParam.Type == targetParam.Type )
                    return Expression.Assign( targetParam, sourceParam );

                return Expression.Assign( targetParam, Expression.Invoke( convert, sourceParam ) );
            }

            return base.LookUpBlock( convert, context.ReferenceTrack, sourceParam, targetParam );
        }
    }
}
