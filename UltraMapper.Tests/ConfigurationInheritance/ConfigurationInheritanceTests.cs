using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            // we set as root a typePair different from (object,object)
            var falseRoot = new TypeMapping( null, new TypePair( typeof( object ), typeof( string ) ) );
            var tree = new TypeMappingInheritanceTree( falseRoot );

            //we add the real root (object,object)
            var trueRoot = new TypePair( typeof( object ), typeof( object ) );
            tree.Add( new TypeMapping( null, trueRoot ) );

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

            //check root is been updated
            Assert.IsTrue( tree.Root.Item.TypePair == trueRoot );

            //no duplicates allowed
            var dup1 = new TypeMapping( null, new TypePair( typeof( object ), typeof( object ) ) );
            var dup2 = new TypeMapping( null, new TypePair( typeof( IEnumerable<IEnumerable<char>> ), typeof( IEnumerable<IEnumerable<char>> ) ) );
            var dup3 = new TypeMapping( null, new TypePair( typeof( object ), typeof( string ) ) );

            tree.Add( dup1 );
            tree.Add( dup2 );
            tree.Add( dup3 );

            var visualizeTree = tree.ToString();

            Assert.IsTrue( visualizeTree.Split( new string[] { dup1.ToString() }, StringSplitOptions.RemoveEmptyEntries ).Length == 1 );
            Assert.IsTrue( visualizeTree.Split( new string[] { dup2.ToString() }, StringSplitOptions.RemoveEmptyEntries ).Length == 1 );
            Assert.IsTrue( visualizeTree.Split( new string[] { dup3.ToString() }, StringSplitOptions.RemoveEmptyEntries ).Length == 1 );

            //var node = new LeafToRootTraversal().Traverse( tree.Root, m => m.CollectionItemEqualityComparer != null );
        }
    }
}
