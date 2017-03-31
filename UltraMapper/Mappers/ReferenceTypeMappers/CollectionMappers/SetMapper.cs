using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Mappers
{
    /*NOTES:
     * 
     *- SortedSet<T> need immediate recursion
     *in order for the item to be added to the collection if T is a 
     *complex type that implements IComparable<T> 
     * 
     * - HashSet<T> need immediate recursion
     *in order for the item to be added to the collection if T is a 
     *complex type that overrides GetHashCode and Equals
     * 
     */

    /// <summary>
    ///Sets need the items to be mapped before adding them to the collection;
    ///otherwise all sets will contain only one item (the one created by the default constructor)
    /// </summary>
    public class SetMapper : CollectionMapper
    {
        public SetMapper( TypeConfigurator configuration )
            : base( configuration ) { this.NeedsImmediateRecursion = true; }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) &&
                target.ImplementsInterface( typeof( ISet<> ) );
        }
    }
}
