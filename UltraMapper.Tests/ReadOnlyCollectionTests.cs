using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ReadOnlyCollectionTests
    {
        //[TestMethod]
        //[TestCategory( "ReadOnlyCollection" )]
        //public void CloneReadOnlyCollection()
        //{
        //    var source = new ReadOnlyCollection<int>( new List<int>() { 1, 2, 3 } );

        //    var ultraMapper = new Mapper();
        //    var target = ultraMapper.Map( source );

        //    Assert.IsTrue( source.SequenceEqual( target ) );
        //}

        //[TestMethod]
        //[TestCategory( "ReadOnlyCollection" )]
        //public void ListToReadOnlyListDifferentElementType()
        //{
        //    List<int> source = Enumerable.Range( 0, 10 ).ToList();
        //    source.Capacity = 100;

        //    var ultraMapper = new Mapper();
        //    var target = ultraMapper.Map<ReadOnlyCollection<double>>( source );

        //    Assert.IsTrue( source.SequenceEqual(
        //        target.Select( item => (int)item ) ) );

        //    bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
        //    Assert.IsTrue( isResultOk );
        //}

        //[TestMethod]
        //[TestCategory( "ReadOnlyCollection" )]
        //public void DirectCollectionToReadOnlyCollection()
        //{
        //    var source = new List<int>() { 1, 2, 3 };

        //    var ultraMapper = new Mapper();
        //    var target = ultraMapper.Map<ReadOnlyCollection<int>>( source );

        //    Assert.IsTrue( source.SequenceEqual( target ) );
        //}
    }
}
