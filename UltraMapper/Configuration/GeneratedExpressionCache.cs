using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace UltraMapper.Internals
{
    public class GeneratedExpressionCache
    {
        private struct TypePairWithOptions
        {
            private static readonly MappingOptionsComparer _mappingOptionsComparer
                = new MappingOptionsComparer();

            public readonly Type SourceType;
            public readonly Type TargetType;
            public readonly IMappingOptions MappingOptions;

            private string _toString;
            private int? _hashcode;

            public TypePairWithOptions( Type source, Type target, IMappingOptions mappingOptions = null )
            {
                this.SourceType = source;
                this.TargetType = target;
                this.MappingOptions = mappingOptions;

                _toString = null;
                _hashcode = null;
            }

            public override bool Equals( object obj )
            {
                if( obj is TypePairWithOptions typePair )
                {
                    return this.SourceType.Equals( typePair.SourceType ) &&
                        this.TargetType.Equals( typePair.TargetType )
                        && _mappingOptionsComparer.Equals( this.MappingOptions, typePair.MappingOptions );
                }

                return false;
            }

            public override int GetHashCode()
            {
                if( _hashcode == null )
                {
                    _hashcode = this.SourceType.GetHashCode()
                        ^ this.TargetType.GetHashCode()
                        ^ _mappingOptionsComparer.GetHashCode( this.MappingOptions );
                }

                return _hashcode.Value;
            }

            public static bool operator !=( TypePairWithOptions obj1, TypePairWithOptions obj2 )
            {
                return !(obj1 == obj2);
            }

            public static bool operator ==( TypePairWithOptions obj1, TypePairWithOptions obj2 )
            {
                return obj1.Equals( obj2 );
            }

            public override string ToString()
            {
                if( _toString == null )
                {
                    StringBuilder sb = new StringBuilder();

                    string sourceTypeName = this.SourceType.GetPrettifiedName();
                    string targetTypeName = this.TargetType.GetPrettifiedName();

                    sb.Append( $"[{sourceTypeName}, {targetTypeName}]" );
                    sb.AppendFormat( " With Options: " );
                    sb.AppendFormat( " {0} = '{1}' ", nameof( IMappingOptions.ReferenceBehavior ), this.MappingOptions?.ReferenceBehavior );
                    sb.AppendFormat( " {0} = '{1}' ", nameof( IMappingOptions.CollectionBehavior ), this.MappingOptions?.CollectionBehavior );

                    _toString = sb.ToString();
                }

                return _toString;
            }
        }

        private readonly Dictionary<TypePairWithOptions, LambdaExpression> _cache =
            new Dictionary<TypePairWithOptions, LambdaExpression>();

        public LambdaExpression Get( Type source, Type target, IMappingOptions options )
        {
            var key = new TypePairWithOptions( source, target, options );
            _cache.TryGetValue( key, out var value );
            return value;
        }

        public void Add( Type source, Type target, IMappingOptions options, LambdaExpression mappingExpression )
        {
            var key = new TypePairWithOptions( source, target, options );
            _cache.Add( key, mappingExpression );
        }
    }
}
