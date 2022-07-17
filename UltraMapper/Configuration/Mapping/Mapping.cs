using System;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public abstract class Mapping : IMapping
    {
        public readonly Configuration GlobalConfig;

        public IMappingSource Source { get; }
        public IMappingTarget Target { get; }

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
                        mapper => mapper.CanHandle( this ) );

                    if( _mapper == null && this.CustomConverter == null )
                    {
                        string sourceTypeName = this.Source.EntryType.GetPrettifiedName();
                        string targetTypeName = this.Target.EntryType.GetPrettifiedName();

                        throw new Exception( $"No object mapper can handle [{sourceTypeName} -> {targetTypeName}]" );
                    }
                }

                return _mapper;
            }
        }

        private LambdaExpression _mappingExpression;
        public virtual LambdaExpression MappingExpression
        {
            get
            {
                if( this.CustomConverter != null )
                    return this.CustomConverter;

                if( _mappingExpression != null )
                    return _mappingExpression;

                _mappingExpression = GlobalConfig.ExpCache.Get( this.Source.EntryType,
                    this.Target.EntryType, (IMappingOptions)this );

                if( _mappingExpression == null )
                {
                    _mappingExpression = this.Mapper.GetMappingExpression( this );

                    GlobalConfig.ExpCache.Add( this.Source.EntryType,
                        this.Target.EntryType, (IMappingOptions)this, _mappingExpression );
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

                var sourceParam = Expression.Parameter( typeof( object ), "sourceInstance" );
                var sourceInstance = Expression.Convert( sourceParam, this.Source.EntryType );

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

                return _mappingFunc = MappingExpressionBuilder.GetMappingFunc(
                   this.Source.EntryType, this.Target.EntryType, this.MappingExpression );
            }
        }

        protected Mapping( Configuration globalConfig, Type sourceType, Type targetType )
        {
            this.GlobalConfig = globalConfig;

            this.Source = new MappingSource( sourceType );
            this.Target = new MappingTarget( targetType );
        }

        protected Mapping( Configuration globalConfig, IMappingSource source, IMappingTarget target )
        {
            GlobalConfig = globalConfig;

            this.Source = source;
            this.Target = target;
        }
    }
}
