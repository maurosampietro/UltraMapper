using System;
using System.Collections.Generic;

namespace UltraMapper
{
    internal class Tree<T>
    {
        private readonly Func<T, T, bool> _nodeSelection;
        public readonly TreeNode<T> Root;

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

            foreach( var node in initialNode.Children )
            {
                if( _nodeSelection.Invoke( node.Item, newElement ) )
                    return this.AddInternal( node, newElement );
            }

            var toRemove = new List<TreeNode<T>>();
            foreach( var node in initialNode.Children )
            {
                if( _nodeSelection.Invoke( newElement, node.Item ) )
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
