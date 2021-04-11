using System.Collections.Generic;
using UltraMapper.Internals;

namespace UltraMapper.Config
{
    internal sealed class ConfigInheritanceNode
    {
        public List<ConfigInheritanceNode> Children { get; set; }
        public ConfigInheritanceNode Parent { get; set; }
        public TypeMapping Item { get; private set; }

        public ConfigInheritanceNode( TypeMapping item )
        {
            this.Children = new List<ConfigInheritanceNode>();
            this.Item = item;
        }
    }
}
