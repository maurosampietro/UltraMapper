using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public sealed class TypeMapping : ITypeOptions, IMapping
    {
        //Each source and target member is instantiated only once per typeMapping
        //so we can handle their options/configuration override correctly.
        private readonly Dictionary<MemberInfo, MappingSource> _sourceProperties
            = new Dictionary<MemberInfo, MappingSource>();

        private readonly Dictionary<MemberInfo, MappingTarget> _targetProperties
            = new Dictionary<MemberInfo, MappingTarget>();

        /*
         *A source member can be mapped to multiple target members.
         *
         *A target member can be mapped just once and for that reason 
         *multiple mappings override each other and the last one is used.
         *
         *The target member can be therefore used as the key of this dictionary
         */
        public readonly Dictionary<MappingTarget, MemberMapping> MemberMappings;
        public readonly Configuration GlobalConfiguration;
        internal TypePair TypePair { get; private set; }

        public MappingResolution MappingResolution { get; internal set; }

        internal TypeMapping( Configuration globalConfig, TypePair typePair )
        {
            this.GlobalConfiguration = globalConfig;
            this.TypePair = typePair;
            this.MemberMappings = new Dictionary<MappingTarget, MemberMapping>();
        }

        private LambdaExpression _customConverter = null;
        public LambdaExpression CustomConverter
        {
            get { return _customConverter; }
            set
            {
                _customConverter = CustomConverterExpressionBuilder.Encapsule( value );
            }
        }

        public LambdaExpression CustomTargetConstructor { get; set; }

        public bool? IgnoreMemberMappingResolvedByConvention { get; set; }
        public ReferenceBehaviors ReferenceBehavior { get; set; } = ReferenceBehaviors.INHERIT;
        public CollectionBehaviors CollectionBehavior { get; set; } = CollectionBehaviors.INHERIT;
        public LambdaExpression CollectionItemEqualityComparer { get; set; }

        private IMappingExpressionBuilder _mapper;
        public IMappingExpressionBuilder Mapper
        {
            get
            {
                if( _mapper == null )
                {
                    _mapper = GlobalConfiguration.Mappers.FirstOrDefault(
                        mapper => mapper.CanHandle( this.TypePair.SourceType, this.TypePair.TargetType ) );

                    if( _mapper == null )
                        throw new Exception( $"No object mapper can handle {this}" );
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

                if( _mappingExpression != null ) return _mappingExpression;

                return _mappingExpression = this.Mapper.GetMappingExpression(
                    this.TypePair.SourceType, this.TypePair.TargetType, this );
            }
        }

        private Func<ReferenceTracker,object, object> _mappingFuncPrimitives;
        public Func<ReferenceTracker, object, object> MappingFuncPrimitives
        {
            get
            {
                if( _mappingFuncPrimitives != null )
                    return _mappingFuncPrimitives;

                var sourceType = this.TypePair.SourceType;
                var targetType = this.TypePair.TargetType;

                var referenceTrackerParam = Expression.Parameter( typeof( ReferenceTracker ), "referenceTracker" );
                var sourceParam = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetParam = Expression.Parameter( typeof( object ), "targetInstance" );

                var sourceInstance = Expression.Convert( sourceParam, sourceType );

                var bodyExp = Expression.Block
                (
                    Expression.Invoke( this.MappingExpression, referenceTrackerParam, sourceInstance )
                );

                return _mappingFuncPrimitives = Expression.Lambda<Func<ReferenceTracker,object, object>>(
                    bodyExp, referenceTrackerParam,sourceParam ).Compile();
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

        public MappingSource GetMappingSource( MemberInfo sourceMember,
            LambdaExpression sourceMemberGetterExpression )
        {
            return _sourceProperties.GetOrAdd( sourceMember,
               () => new MappingSource( sourceMemberGetterExpression ) );
        }

        public MappingTarget GetMappingTarget( MemberInfo targetMember,
            LambdaExpression targetMemberGetter, LambdaExpression targetMemberSetter )
        {
            return _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberSetter, targetMemberGetter ) );
        }

        public MappingSource GetMappingSource( MemberInfo sourceMember,
            MemberAccessPath sourceMemberPath )
        {
            return _sourceProperties.GetOrAdd( sourceMember,
                () => new MappingSource( sourceMemberPath ) );
        }

        public MappingTarget GetMappingTarget( MemberInfo targetMember,
            MemberAccessPath targetMemberPath )
        {
            return _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberPath ) );
        }
    }
}
