using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Tests
{
    [TestClass]
    public class CollectionTests
    {
        private static Random _random = new Random();

        //IComparable is required to test sorted collections
        private class ComplexType : IComparable<ComplexType>
        {
            public int A { get; set; }

            public int CompareTo( ComplexType other )
            {
                return this.A.CompareTo( other.A );
            }

            public override int GetHashCode()
            {
                return this.A;
            }

            public override bool Equals( object obj )
            {
                return this.A.Equals( (obj as ComplexType)?.A );
            }
        }

        private class GenericCollectionsPrimitiveArgument
        {
            public List<int> List { get; set; }
            public HashSet<int> HashSet { get; set; }
            public SortedSet<int> SortedSet { get; set; }
            public Stack<int> Stack { get; set; }
            public Queue<int> Queue { get; set; }
            public LinkedList<int> LinkedList { get; set; }
            public ObservableCollection<int> ObservableCollection { get; set; }

            public GenericCollectionsPrimitiveArgument()
            {
                this.List = new List<int>();
                this.HashSet = new HashSet<int>();
                this.SortedSet = new SortedSet<int>();
                this.Stack = new Stack<int>();
                this.Queue = new Queue<int>();
                this.LinkedList = new LinkedList<int>();
                this.ObservableCollection = new ObservableCollection<int>();
            }
        }

        private class GenericCollectionsComplexArgument
        {
            public List<ComplexType> List { get; set; }
            public HashSet<ComplexType> HashSet { get; set; }
            public SortedSet<ComplexType> SortedSet { get; set; }
            public Stack<ComplexType> Stack { get; set; }
            public Queue<ComplexType> Queue { get; set; }
            public LinkedList<ComplexType> LinkedList { get; set; }
            public ObservableCollection<ComplexType> ObservableCollection { get; set; }

            public GenericCollectionsComplexArgument()
            {
                this.List = new List<ComplexType>();
                this.HashSet = new HashSet<ComplexType>();
                this.SortedSet = new SortedSet<ComplexType>();
                this.Stack = new Stack<ComplexType>();
                this.Queue = new Queue<ComplexType>();
                this.LinkedList = new LinkedList<ComplexType>();
                this.ObservableCollection = new ObservableCollection<ComplexType>();
            }
        }

        [TestMethod]
        public void CloneCollectionPrimitiveArgument()
        {
            var source = new GenericCollectionsPrimitiveArgument();
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( i );
                source.HashSet.Add( i );
                source.SortedSet.Add( i );
                source.Stack.Push( i );
                source.Queue.Enqueue( i );
                source.LinkedList.AddLast( i );
                source.ObservableCollection.Add( i );
            }

            var target = new GenericCollectionsPrimitiveArgument();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void CloneCollectionComplexArgument()
        {
            var source = new GenericCollectionsComplexArgument();
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollectionsComplexArgument();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}
