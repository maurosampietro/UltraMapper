using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Collections.ObjectModel;
using UltraMapper.Internals;
using BenchmarkDotNet.Configs;

namespace UltraMapper.Benchmarks
{
    internal class Program
    {
        static void Main( string[] args )
        {
            var summary = BenchmarkRunner.Run<MappersBenchmark>( new DebugInProcessConfig() );
        }
    }

    [SimpleJob( RuntimeMoniker.Net50 )]
    [SimpleJob( RuntimeMoniker.Net60 )]
    public class MappersBenchmark
    {
        private readonly AutoMapper.Mapper _autoMapper;
        private readonly Mapper _ultraMapper;
        private readonly GenericCollections<ComplexType> _source;

        //IComparable is required to test sorted collections
        protected class ComplexType : IComparable<ComplexType>
        {
            public int A { get; set; }
            public InnerType InnerType { get; set; }

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
                if( !(obj is ComplexType otherObj) ) return false;

                return this.A.Equals( otherObj?.A ) &&
                    (this.InnerType == null && otherObj.InnerType == null) ||
                    ((this.InnerType != null && otherObj.InnerType != null) &&
                        this.InnerType.Equals( otherObj.InnerType ));
            }
        }

        protected class InnerType
        {
            public string String { get; set; }
        }

        protected class GenericCollections<T>
        {
            public T[] Array { get; set; }
            public HashSet<T> HashSet { get; set; }
            public SortedSet<T> SortedSet { get; set; }
            public List<T> List { get; set; }
            public Stack<T> Stack { get; set; }
            public Queue<T> Queue { get; set; }
            public LinkedList<T> LinkedList { get; set; }
            public ObservableCollection<T> ObservableCollection { get; set; }

            public GenericCollections( bool initializeIfPrimitiveGenericArg, uint minVal = 0, uint maxVal = 10 )
            {
                if( minVal > maxVal )
                    throw new ArgumentException( $"{nameof( maxVal )} must be a value greater or equal to {nameof( minVal )}" );

                this.Array = new T[ maxVal - minVal ];
                this.List = new List<T>();
                this.HashSet = new HashSet<T>();
                this.SortedSet = new SortedSet<T>();
                this.Stack = new Stack<T>();
                this.Queue = new Queue<T>();
                this.LinkedList = new LinkedList<T>();
                this.ObservableCollection = new ObservableCollection<T>();

                if( initializeIfPrimitiveGenericArg )
                    Initialize( minVal, maxVal );
            }

            private void Initialize( uint minval, uint maxval )
            {
                var elementType = typeof( T );
                if( elementType.IsBuiltIn( true ) )
                {
                    for( uint i = 0, v = minval; v < maxval; i++, v++ )
                    {
                        T value = (T)Convert.ChangeType( v,
                            elementType.GetUnderlyingTypeIfNullable() );

                        this.Array[ i ] = value;
                        this.List.Add( value );
                        this.HashSet.Add( value );
                        this.SortedSet.Add( value );
                        this.Stack.Push( value );
                        this.Queue.Enqueue( value );
                        this.LinkedList.AddLast( value );
                        this.ObservableCollection.Add( value );
                    }
                }
            }
        }

        private class ReadOnlyGeneric<T>
        {
            public ReadOnlyCollection<T> Array { get; set; }
            public ReadOnlyCollection<T> HashSet { get; set; }
            public ReadOnlyCollection<T> SortedSet { get; set; }
            public ReadOnlyCollection<T> List { get; set; }
            public ReadOnlyCollection<T> Stack { get; set; }
            public ReadOnlyCollection<T> Queue { get; set; }
            public ReadOnlyCollection<T> LinkedList { get; set; }
            public ReadOnlyCollection<T> ObservableCollection { get; set; }
        }

        public MappersBenchmark()
        {
            var config = new MapperConfiguration( cfg =>
            {
                cfg.CreateMap<GenericCollections<ComplexType>, ReadOnlyGeneric<ComplexType>>();
                cfg.CreateMap<ComplexType, ComplexType>();
                cfg.CreateMap<InnerType, InnerType>();
            } );
            _autoMapper = new AutoMapper.Mapper( config );

            _ultraMapper = new Mapper( cfg => cfg.IsReferenceTrackingEnabled = true );

            var innerType = new InnerType() { String = "test" };

            _source = new GenericCollections<ComplexType>( false, 0, 1000 );
            for( int i = 0; i < 1000; i++ )
            {
                _source.Array[ i ] = new ComplexType() { A = i, InnerType = innerType };
                _source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                _source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                _source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                _source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
                _source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
                _source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
                _source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
            }
        }

        [Benchmark]
        public void UltraMapperTest()
        {
            var target = new ReadOnlyGeneric<ComplexType>();
            _ultraMapper.Map( _source, target );
        }

        [Benchmark]
        public void AutoMapperTest()
        {
            var target = new ReadOnlyGeneric<ComplexType>();
            _autoMapper.Map( _source, target );
        }
    }
}
