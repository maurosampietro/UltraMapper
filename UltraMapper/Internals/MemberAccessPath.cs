using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace UltraMapper.Internals
{
    public class MemberAccessPath : IEnumerable<MemberInfo>
    {
        private readonly List<MemberInfo> _memberAccess
            = new List<MemberInfo>();

        public int Count { get { return _memberAccess.Count; } }

        public void Add( MemberInfo memberInfo )
            => _memberAccess.Add( memberInfo );

        public MemberAccessPath Reverse()
        {
            _memberAccess.Reverse();
            return this;
        }

        public IEnumerator<MemberInfo> GetEnumerator()
            => _memberAccess.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
