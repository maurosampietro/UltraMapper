using System;
using System.Collections.Generic;
using System.Text;
using UltraMapper.Internals;

namespace UltraMapper
{
    internal class TypeMappingInheritanceTree : Tree<TypeMapping>
    {
        private readonly Dictionary<TypePair, TreeNode<TypeMapping>> _nodeDictionary
            = new Dictionary<TypePair, TreeNode<TypeMapping>>();

        public TypeMappingInheritanceTree( TypeMapping root )
                 : base( root, ( s, t ) => s.TypePair.SourceType.IsAssignableFrom( t.TypePair.SourceType ) &&
                    s.TypePair.TargetType.IsAssignableFrom( t.TypePair.TargetType ) )
        {
            _nodeDictionary.Add( root.TypePair, new TreeNode<TypeMapping>( root ) );
        }

        public TreeNode<TypeMapping> this[ TypePair key ]
        {
            get { return _nodeDictionary[ key ]; }
        }

        public override void Add( TypeMapping element )
        {
            var key = element.TypePair;
            if( !_nodeDictionary.ContainsKey( key ) )
            {
                _nodeDictionary.Add( key, new TreeNode<TypeMapping>( element ) );
                base.Add( element );
            }
        }

        public TreeNode<TypeMapping> GetOrAdd( TypePair typePair, Func<TypeMapping> valueFactory )
        {
            TreeNode<TypeMapping> value;
            if( !_nodeDictionary.TryGetValue( typePair, out value ) )
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
            ToStringInternal( stringBuilder, 0, this.Root );

            return stringBuilder.ToString();
        }

        private void ToStringInternal( StringBuilder stringBuilder, int tabs, TreeNode<TypeMapping> initialNode )
        {
            stringBuilder.Append( new String( '\t', tabs++ ) );
            stringBuilder.AppendLine( initialNode.Item.TypePair.ToString() );

            foreach( var node in initialNode.Children )
                ToStringInternal( stringBuilder, tabs, node );
        }
    }
}
