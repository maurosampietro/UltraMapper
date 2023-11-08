﻿using System;
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

        private UltraMapperDelegate _mappingFunc;
        public UltraMapperDelegate MappingFunc
        {
            get
            {
                if( _mappingFunc != null ) return _mappingFunc;

                return _mappingFunc = MappingExpressionBuilder.GetMappingEntryPoint(
                   this.Source.EntryType, this.Target.EntryType, this.MappingExpression );
            }
        }

        private bool? _needsRuntimeTypeInstepction = null;
        public bool NeedsRuntimeTypeInspection => _needsRuntimeTypeInstepction ??= Source.ReturnType.IsInterface || Source.ReturnType.IsAbstract;

        //protected Mapping( Configuration config, Type sourceType, Type targetType )
        //{
        //    this.GlobalConfig = config;

        //    this.Source = new MappingSource( sourceType );
        //    this.Target = new MappingTarget( targetType );
        //}

        protected Mapping( Configuration config, IMappingSource source, IMappingTarget target )
        {
            GlobalConfig = config;

            this.Source = source;
            this.Target = target;
        }
    }
}
