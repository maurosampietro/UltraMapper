using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltraMapper.Internals;

namespace UltraMapper.Tests.ConfigurationInheritance
{
    [TestClass]
    public class ConfigurationInheritanceTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            var tree = new TypeMappingInheritanceTree( new TypeMapping( null, new TypePair( typeof( object ), typeof( string ) ) ) );

            tree.Add( new TypeMapping( null, new TypePair( typeof( Dictionary<string, string> ), typeof( Dictionary<string, string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( ObservableCollection<string> ), typeof( IList<string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( IList<string> ), typeof( IList<string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( ICollection<string> ), typeof( ICollection<string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( Collection<string> ), typeof( Collection<string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( List<string> ), typeof( List<string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( IEnumerable<string> ), typeof( IEnumerable<string> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( IEnumerable<char> ), typeof( IEnumerable<char> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( IEnumerable<IEnumerable<char>> ), typeof( IEnumerable<IEnumerable<char>> ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( object ), typeof( string ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( string ), typeof( string ) ) ) );
            tree.Add( new TypeMapping( null, new TypePair( typeof( object ), typeof( object ) ) ) );

            var visualizeTree = tree.ToString();

            var node = new LeafToRootTraversal().Traverse( tree.Root, m => m.CollectionItemEqualityComparer != null );

        }
    }
}
