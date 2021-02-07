using System;
using System.Collections.Generic;

namespace UltraMapper.Config
{
    internal class Tree<T>
    {
        private readonly Func<T, T, bool> _nodeSelection;
        public TreeNode<T> Root { get; private set; }

        public Tree( T root, Func<T, T, bool> nodeSelection )
        {
            _nodeSelection = nodeSelection;
            this.Root = new TreeNode<T>( root );
        }

        public virtual TreeNode<T> Add( T element )
        {
            return this.AddInternal( this.Root, element );
        }

        private TreeNode<T> AddInternal( TreeNode<T> initialNode, T newElement )
        {
            //if( initialNode.Item.TypePair == newElement.TypePair )
            //    return;

            //root swap
            if( initialNode == this.Root && _nodeSelection( newElement, initialNode.Item ) )
            {
                this.Root = new TreeNode<T>( newElement )
                {
                    Parent = initialNode.Parent,
                };

                this.Root.Children.Add( initialNode );
                initialNode.Parent = this.Root;

                return this.Root;
            }

            foreach( var node in initialNode.Children )
            {
                if( _nodeSelection( node.Item, newElement ) )
                    return this.AddInternal( node, newElement );
            }

            var toRemove = new List<TreeNode<T>>();
            foreach( var node in initialNode.Children )
            {
                if( _nodeSelection( newElement, node.Item ) )
                    toRemove.Add( node );
            }

            var newNode = new TreeNode<T>( newElement )
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
    }
}
