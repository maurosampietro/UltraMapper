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
            s.TypePair.SourceType.IsAssignableFrom( t.TypePair.SourceType ) &&
            s.TypePair.TargetType.IsAssignableFrom( t.TypePair.TargetType );

        public TypeMappingInheritanceTree( TypeMapping root )
                 : base( root, _parentChildRelation )
        {
            _nodeDictionary.Add( root.TypePair, new TreeNode<TypeMapping>( root ) );
        }

        public TreeNode<TypeMapping> this[ TypePair key ]
        {
            get { return _nodeDictionary[ key ]; }
        }

        public override TreeNode<TypeMapping> Add( TypeMapping element )
        {
            var key = element.TypePair;
            if( !_nodeDictionary.TryGetValue( key, out TreeNode<TypeMapping> value ) )
            {
                value = base.Add( element );
                _nodeDictionary.Add( key, value );
            }

            return value;
        }

        public TreeNode<TypeMapping> GetOrAdd( TypePair typePair, Func<TypeMapping> valueFactory )
        {
            if( !_nodeDictionary.TryGetValue( typePair, out TreeNode<TypeMapping> value ) )
            {
                var element = valueFactory.Invoke();
                this.Add( element );

                return this[ typePair ];
            }

            return value;
        }

        public bool ContainsKey( TypePair key )
        {
            return _nodeDictionary.ContainsKey( key );
        }

        public bool TryGetValue( TypePair key, out TreeNode<TypeMapping> value )
        {
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
            stringBuilder.Append( new String( '\t', indentationLevel++ ) );
            stringBuilder.AppendLine( initialNode.Item.TypePair.ToString() );

            foreach( var node in initialNode.Children )
                this.ToStringInternal( stringBuilder, indentationLevel, node );
        }
    }
}
