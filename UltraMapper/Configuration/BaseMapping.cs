using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public abstract class BaseMapping : IMapping
    {
        public readonly Configuration GlobalConfig;
        public readonly TypePair TypePair;

        public MappingResolution MappingResolution { get; internal set; }

        public abstract LambdaExpression CustomConverter { get; set; }

        private IMappingExpressionBuilder _mapper;
        public IMappingExpressionBuilder Mapper
        {
            get
            {
                if( _mapper == null )
                {
                    _mapper = GlobalConfig.Mappers.FirstOrDefault(
                        mapper => mapper.CanHandle( this.TypePair.SourceType, this.TypePair.TargetType ) );

                    if( _mapper == null && this.CustomConverter == null )
                        throw new Exception( $"No object mapper can handle {this.TypePair}" );
                }

                return _mapper;
            }
        }

        private LambdaExpression _mappingExpression;
        public LambdaExpression MappingExpression
        {
            get
            {
                if( this.CustomConverter != null )
                    return this.CustomConverter;

                if( _mappingExpression != null )
                    return _mappingExpression;

                var sourceType = this.TypePair.SourceType;
                var targetType = this.TypePair.TargetType;

                _mappingExpression =GlobalConfig.ExpCache.Get( sourceType, targetType, (IMappingOptions)this );
                if( _mappingExpression == null )
                {
                    _mappingExpression = this.Mapper.GetMappingExpression(
                          sourceType, targetType, (IMappingOptions)this );

                    GlobalConfig.ExpCache.Add( sourceType, targetType, (IMappingOptions)this, _mappingExpression );
                }

                return _mappingExpression;
            }
        }

        private Func<ReferenceTracker, object, object> _mappingFuncPrimitives;
        public Func<ReferenceTracker, object, object> MappingFuncPrimitives
        {
            get
            {
                if( _mappingFuncPrimitives != null )
                    return _mappingFuncPrimitives;

                var referenceTrackerParam = Expression.Parameter( typeof( ReferenceTracker ), "referenceTracker" );

                var sourceType = this.TypePair.SourceType;
                var sourceParam = Expression.Parameter( typeof( object ), "sourceInstance" );
                var sourceInstance = Expression.Convert( sourceParam, sourceType );

                Expression bodyExp;
                if( this.MappingExpression.Parameters.Count == 1 )
                    bodyExp = Expression.Convert( Expression.Invoke( this.MappingExpression, sourceInstance ), typeof( object ) );

                else if( this.MappingExpression.Parameters.Count == 2 )
                    bodyExp = Expression.Invoke( this.MappingExpression, referenceTrackerParam, sourceInstance );

                else throw new NotSupportedException( "Unsupported number of arguments" );

                return _mappingFuncPrimitives = Expression.Lambda<Func<ReferenceTracker, object, object>>(
                    bodyExp, referenceTrackerParam, sourceParam ).Compile();
            }
        }

        private Action<ReferenceTracker, object, object> _mappingFunc;
        public Action<ReferenceTracker, object, object> MappingFunc
        {
            get
            {
                if( _mappingFunc != null ) return _mappingFunc;

                var sourceType = this.TypePair.SourceType;
                var targetType = this.TypePair.TargetType;

                return _mappingFunc = MappingExpressionBuilder.GetMappingFunc(
                   sourceType, targetType, this.MappingExpression );
            }
        }

        protected BaseMapping( Configuration globalConfig, TypePair typePair )
        {
            this.GlobalConfig = globalConfig;
            this.TypePair = typePair;
        }
    }
}
