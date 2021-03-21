using System;
using System.Collections.Generic;
using System.Text;
using UltraMapper.Internals;

namespace UltraMapper.Config
{
    internal class TypeMappingInheritanceTree : Tree<TypeMapping>
    {
        private readonly Dictionary<TypePair, TreeNode<TypeMapping>> _nodeDictionary
            = new Dictionary<TypePair, TreeNode<TypeMapping>>();

        private static readonly Func<TypeMapping, TypeMapping, bool> _parentChildRelation = ( s, t ) =>
            s.SourceType.IsAssignableFrom( t.SourceType ) &&
            s.TargetType.IsAssignableFrom( t.TargetType );

        public TypeMappingInheritanceTree( TypeMapping root )
                 : base( root, _parentChildRelation )
        {
            var typePair = new TypePair( root.SourceType, root.TargetType );
            _nodeDictionary.Add( typePair, new TreeNode<TypeMapping>( root ) );
        }

        public TreeNode<TypeMapping> this[ Type sourceType, Type targetType ]
        {
            get
            {
                var typePair = new TypePair( sourceType, targetType );
                return _nodeDictionary[ typePair ];
            }
        }

        public override TreeNode<TypeMapping> Add( TypeMapping element )
        {
            var key = new TypePair( element.SourceType, element.TargetType );
            if( !_nodeDictionary.TryGetValue( key, out TreeNode<TypeMapping> value ) )
            {
                value = base.Add( element );
                _nodeDictionary.Add( key, value );
            }

            return value;
        }

        public TreeNode<TypeMapping> GetOrAdd( Type sourceType, Type targetType, Func<TypeMapping> valueFactory )
        {
            var typePair = new TypePair( sourceType, targetType );
            if( !_nodeDictionary.TryGetValue( typePair, out TreeNode<TypeMapping> value ) )
            {
                var element = valueFactory.Invoke();
                return this.Add( element );
            }

            return value;
        }

        public bool ContainsKey( Type sourceType, Type targetType )
        {
            var key = new TypePair( sourceType, targetType );
            return _nodeDictionary.ContainsKey( key );
        }

        public bool TryGetValue( Type sourceType, Type targetType, out TreeNode<TypeMapping> value )
        {
            var key = new TypePair( sourceType, targetType );
            return _nodeDictionary.TryGetValue( key, out value );
        }

        public override string ToString()
        {
            if( this.Root == null ) return "{}";

            var stringBuilder = new StringBuilder();
            this.ToStringInternal( stringBuilder, 0, this.Root );

            return stringBuilder.ToString();
        }

        private void ToStringInternal( StringBuilder stringBuilder, int indentationLevel, TreeNode<TypeMapping> initialNode )
        {
            string sourceTypeName = initialNode.Item.SourceType.GetPrettifiedName();
            string targetTypeName = initialNode.Item.TargetType.GetPrettifiedName();

            stringBuilder.Append( new String( '\t', indentationLevel++ ) );
            stringBuilder.AppendLine( $"[{sourceTypeName} -> {targetTypeName}" );

            foreach( var node in initialNode.Children )
                this.ToStringInternal( stringBuilder, indentationLevel, node );
        }
    }
}