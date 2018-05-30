using System;
using System.Collections.Generic;
using System.Text;
using UltraMapper.Internals;

namespace UltraMapper
{
    internal sealed class TreeNode<T>
    {
        public List<TreeNode<T>> Children { get; set; }
        public TreeNode<T> Parent { get; set; }
        public T Item { get; private set; }

        public TreeNode( T item )
        {
            this.Children = new List<TreeNode<T>>();
            this.Item = item;
        }
    }
}
