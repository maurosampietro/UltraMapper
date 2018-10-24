using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Internals.ExtensionMethods;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CollectionMapperContext : ReferenceMapperContext
    {
        public Type SourceCollectionElementType { get; set; }
        public Type TargetCollectionElementType { get; set; }

        public bool IsSourceElementTypeBuiltIn { get; set; }
        public bool IsTargetElementTypeBuiltIn { get; set; }

        public bool IsSourceElementTypeStruct { get; set; }
        public bool IsTargetElementTypeStruct { get; set; }

        public ParameterExpression SourceCollectionLoopingVar { get; set; }
        public Expression UpdateCollection { get; internal set; }

        public CollectionMapperContext( Type source, Type target, IMappingOptions options )
            : base( source, target, options )
        {
            SourceCollectionElementType = SourceInstance.Type.GetCollectionGenericType();
            TargetCollectionElementType = TargetInstance.Type.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceCollectionElementType.IsBuiltInType( true );
            IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltInType( true );

            IsSourceElementTypeStruct = !SourceCollectionElementType.IsClass;
            IsTargetElementTypeStruct = !TargetCollectionElementType.IsClass;

            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );

            if( options.CollectionItemEqualityComparer != null )
            {
                var updateCollectionMethodInfo = typeof( LinqExtensions ).GetMethod(
                    nameof( LinqExtensions.Update ), BindingFlags.Static | BindingFlags.NonPublic )
                    .MakeGenericMethod( SourceCollectionElementType, TargetCollectionElementType );

                UpdateCollection = Expression.Call( null, updateCollectionMethodInfo, Mapper, ReferenceTracker, SourceInstance,
                   Expression.Convert( TargetInstance, typeof( ICollection<> ).MakeGenericType( TargetCollectionElementType ) ),
                   Expression.Convert( Expression.Constant( options.CollectionItemEqualityComparer.Compile() ),
                        typeof( Func<,,> ).MakeGenericType( SourceCollectionElementType, TargetCollectionElementType, typeof( bool ) ) ) );
            }
        }
    }
}
