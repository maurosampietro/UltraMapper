using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper
{
    internal class LeafToRootTraversal
    {
        public TreeNode<T> Traverse<T>( TreeNode<T> node, Func<T, bool> nodeSelectionCheck )
        {
            if( node == null ) return null;

            if( nodeSelectionCheck( node.Item ) )
                return node;

            while( node.Parent != null )
            {
                node = node.Parent;

                if( nodeSelectionCheck( node.Item ) )
                    return node;
            }

            return null;
        }
    }
}
