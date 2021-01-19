using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UltraMapper.Internals
{
    public class MemberAccessPath : IEnumerable<MemberInfo>
    {
        private readonly List<MemberInfo> _memberAccess
            = new List<MemberInfo>();

        public int Count => _memberAccess.Count;

        public MemberAccessPath() { }

        public MemberAccessPath( MemberInfo memberInfo )
            : this( new[] { memberInfo } ) { }

        public MemberAccessPath( IEnumerable<MemberInfo> members )
        {
            foreach( var member in members )
                _memberAccess.Add( member );
        }

        public void Add( MemberInfo memberInfo )
            => _memberAccess.Add( memberInfo );

        public MemberInfo this[ int index ] => _memberAccess[ index ];

        public MemberAccessPath Reverse()
        {
            _memberAccess.Reverse();
            return this;
        }

        public IEnumerator<MemberInfo> GetEnumerator()
            => _memberAccess.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        public override bool Equals( object obj )
        {
            if( obj is MemberAccessPath accessPath )
                return this.GetHashCode() == accessPath.GetHashCode();

            return false;
        }

        public override int GetHashCode()
        {
            return this.Select( i => i.GetHashCode() )
                .Aggregate( 0, ( aggregate, next ) => aggregate ^ next );
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for( int i = 0; i < this.Count - 1; i++ )
                sb.Append( $"{this[ i ].Name} -> " );
          
            sb.Append( $"{this[ this.Count - 1 ].Name}" );
            return sb.ToString();
        }
    }
}
