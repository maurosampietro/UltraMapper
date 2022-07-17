using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltraMapper.Config;
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
            var falseRoot = new TypeMapping( null, typeof( object ), typeof( string ) );
            var tree = new ConfigInheritanceTree( falseRoot );

            //we add the real root (object,object)
            tree.Add( new TypeMapping( null, typeof( object ), typeof( object ) ) );

            tree.Add( new TypeMapping( null, typeof( Dictionary<string, string> ), typeof( Dictionary<string, string> ) ) );
            tree.Add( new TypeMapping( null, typeof( ObservableCollection<string> ), typeof( IList<string> ) ) );
            tree.Add( new TypeMapping( null, typeof( IList<string> ), typeof( IList<string> ) ) );
            tree.Add( new TypeMapping( null, typeof( ICollection<string> ), typeof( ICollection<string> ) ) );
            tree.Add( new TypeMapping( null, typeof( Collection<string> ), typeof( Collection<string> ) ) );
            tree.Add( new TypeMapping( null, typeof( List<string> ), typeof( List<string> ) ) );
            tree.Add( new TypeMapping( null, typeof( IEnumerable<string> ), typeof( IEnumerable<string> ) ) );
            tree.Add( new TypeMapping( null, typeof( IEnumerable<char> ), typeof( IEnumerable<char> ) ) );
            tree.Add( new TypeMapping( null, typeof( IEnumerable<IEnumerable<char>> ), typeof( IEnumerable<IEnumerable<char>> ) ) );
            tree.Add( new TypeMapping( null, typeof( object ), typeof( string ) ) );
            tree.Add( new TypeMapping( null, typeof( string ), typeof( string ) ) );

            //check root is been updated
            Assert.IsTrue( tree.Root.Item.Source.EntryType == typeof( object ) );
            Assert.IsTrue( tree.Root.Item.Target.EntryType == typeof( object ) );

            //no duplicates allowed
            var dup1 = new TypeMapping( null, typeof( object ), typeof( object ) );
            var dup2 = new TypeMapping( null, typeof( IEnumerable<IEnumerable<char>> ), typeof( IEnumerable<IEnumerable<char>> ) );
            var dup3 = new TypeMapping( null, typeof( object ), typeof( string ) );

            tree.Add( dup1 );
            tree.Add( dup2 );
            tree.Add( dup3 );
        }
    }
}
