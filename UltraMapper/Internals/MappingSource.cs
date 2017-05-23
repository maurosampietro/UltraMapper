using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Internals
{
    public class MappingSource : MappingMemberBase
    {
        public LambdaExpression ValueGetter { get; set; }

        internal MappingSource( MemberAccessPath memberGetter )
            : base( memberGetter )
        {
            this.ValueGetter = memberGetter.GetGetterLambdaExpression();
        }

        internal MappingSource( LambdaExpression memberGetter )
            : base( memberGetter.ExtractMember() )
        {
            this.ValueGetter = memberGetter;
        }
    }
}
