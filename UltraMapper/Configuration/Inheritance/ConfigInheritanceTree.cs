using System;
using System.Collections.Generic;
using System.Text;
using UltraMapper.Internals;

namespace UltraMapper.Config
{
    internal class ConfigInheritanceTree
    {
        public ConfigInheritanceNode Root { get; private set; }

        private readonly Dictionary<TypePair, ConfigInheritanceNode> _nodeDictionary
            = new Dictionary<TypePair, ConfigInheritanceNode>();

        public ConfigInheritanceTree( TypeMapping root )
        {
            var typePair = new TypePair( root.Source.EntryType, root.Target.EntryType );
            _nodeDictionary.Add( typePair, new ConfigInheritanceNode( root ) );

            this.Root = new ConfigInheritanceNode( root );
        }

        public ConfigInheritanceNode this[ Type sourceType, Type targetType ]
        {
            get
            {
                var typePair = new TypePair( sourceType, targetType );
                return _nodeDictionary[ typePair ];
            }
        }

        public ConfigInheritanceNode Add( TypeMapping element )
        {
            var key = new TypePair( element.Source.EntryType, element.Target.EntryType );
            if( !_nodeDictionary.TryGetValue( key, out ConfigInheritanceNode value ) )
            {
                value = this.AddInternal( this.Root, element );
                _nodeDictionary.Add( key, value );
            }

            return value;
        }

        private ConfigInheritanceNode AddInternal( ConfigInheritanceNode initialNode, TypeMapping newElement )
        {
            //if( initialNode.Item.TypePair == newElement.TypePair )
            //    return;

            //root swap
            if( initialNode == this.Root && IsParentChildRelation( newElement, initialNode.Item ) )
            {
                this.Root = new ConfigInheritanceNode( newElement )
                {
                    Parent = initialNode.Parent,
                };

                this.Root.Children.Add( initialNode );
                initialNode.Parent = this.Root;

                return this.Root;
            }

            foreach( var node in initialNode.Children )
            {
                if( IsParentChildRelation( node.Item, newElement ) )
                    return this.AddInternal( node, newElement );
            }

            var toRemove = new List<ConfigInheritanceNode>();
            foreach( var node in initialNode.Children )
            {
                if( IsParentChildRelation( newElement, node.Item ) )
                    toRemove.Add( node );
            }

            var newNode = new ConfigInheritanceNode( newElement )
            {
                Parent = initialNode
            };

            foreach( var tr in toRemove )
            {
                initialNode.Children.Remove( tr );
                newNode.Children.Add( tr );
                tr.Parent = newNode;
            }

            initialNode.Children.Add( newNode );
            return newNode;
        }

        public TypeMapping GetOrAdd( Type sourceType, Type targetType, Func<TypeMapping> valueFactory )
        {
            var typePair = new TypePair( sourceType, targetType );
            if( !_nodeDictionary.TryGetValue( typePair, out ConfigInheritanceNode value ) )
            {
                var element = valueFactory.Invoke();
                value = this.Add( element );
            }

            return value.Item;//[ options ];
        }

        //public bool ContainsKey( Type sourceType, Type targetType )
        //{
        //    var key = new TypePair( sourceType, targetType );
        //    return _nodeDictionary.ContainsKey( key );
        //}

        public bool TryGetValue( Type sourceType, Type targetType, out ConfigInheritanceNode value )
        {
            var key = new TypePair( sourceType, targetType );
            return _nodeDictionary.TryGetValue( key, out value );
        }

        public override string ToString() => this.ToString( false, false );

        public string ToString( bool includeMembers, bool includeOptions )
        {
            if( this.Root == null ) return "{}";

            var stringBuilder = new StringBuilder();
            this.ToStringInternal( stringBuilder, 0, this.Root, includeMembers, includeOptions );

            return stringBuilder.ToString();
        }

        private void ToStringInternal( StringBuilder stringBuilder, int indentationLevel,
            ConfigInheritanceNode initialNode, bool includeMembers, bool includeOptions )
        {
            string sourceTypeName = initialNode.Item.Source.EntryType.GetPrettifiedName();
            string targetTypeName = initialNode.Item.Target.EntryType.GetPrettifiedName();

            StringBuilder options = new StringBuilder();
            if( includeOptions )
            {
                options.Append( $"{nameof( IMappingOptions.ReferenceBehavior )}: {initialNode.Item.ReferenceBehavior}, " );
                options.Append( $"{nameof( IMappingOptions.CollectionBehavior )}: {initialNode.Item.CollectionBehavior}" );

                if( initialNode.Item.CustomTargetConstructor != null )
                    options.Append( $" ,{nameof( IMappingOptions.CustomTargetConstructor )}: PROVIDED" );

                if( initialNode.Item.CustomConverter != null )
                    options.Append( $" ,{nameof( IMappingOptions.CustomConverter )}: PROVIDED" );

                if( initialNode.Item.CollectionItemEqualityComparer != null )
                    options.Append( $" ,{nameof( IMappingOptions.CollectionItemEqualityComparer )}: PROVIDED" );
            }

            stringBuilder.Append( new String( '\t', indentationLevel++ ) );
            stringBuilder.AppendLine( $"[{sourceTypeName} -> {targetTypeName}] ({options})" );

            if( includeMembers )
            {
                var indent = new String( '\t', indentationLevel++ );
                StringBuilder memberOptions = new StringBuilder();

                foreach( var item in initialNode.Item.MemberToMemberMappings.Values )
                {
                    if( includeOptions )
                    {
                        memberOptions.Append( $"{nameof( IMappingOptions.ReferenceBehavior )}: {item.ReferenceBehavior}, " );
                        memberOptions.Append( $"{nameof( IMappingOptions.CollectionBehavior )}: {item.CollectionBehavior}" );

                        if( item.CustomTargetConstructor != null )
                            memberOptions.Append( $" ,{nameof( IMappingOptions.CustomTargetConstructor )}: PROVIDED" );

                        if( item.CustomConverter != null )
                            memberOptions.Append( $" ,{nameof( IMappingOptions.CustomConverter )}: PROVIDED" );

                        if( item.CollectionItemEqualityComparer != null )
                            memberOptions.Append( $" ,{nameof( IMappingOptions.CollectionItemEqualityComparer )}: PROVIDED" );
                    }

                    stringBuilder.AppendLine( $"{indent}{item} ({memberOptions})" );
                    memberOptions.Clear();
                }

                indentationLevel--;
            }

            foreach( var node in initialNode.Children )
                this.ToStringInternal( stringBuilder, indentationLevel, node, includeMembers, includeOptions );
        }

        private bool IsParentChildRelation( TypeMapping s, TypeMapping t )
        {
            return s.Source.EntryType.IsAssignableFrom( t.Source.EntryType ) &&
                s.Target.EntryType.IsAssignableFrom( t.Target.EntryType );
        }
    }
}