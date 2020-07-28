using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UltraMapper.Internals
{
    public class MemberAccessPath : IEnumerable<MemberInfo>
    {
        private readonly List<MemberInfo> _memberAccess
            = new List<MemberInfo>();

        public int Count { get { return _memberAccess.Count; } }

        public MemberAccessPath() { }

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
