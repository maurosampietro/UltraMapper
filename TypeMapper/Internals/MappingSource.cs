using System.Linq.Expressions;

namespace TypeMapper.Internals
{
    public class MappingSource : MappingMemberBase
    {
        public LambdaExpression ValueGetter { get; set; }

        internal MappingSource( LambdaExpression memberGetter )
            : base( memberGetter.ExtractMember() )
        {
            this.ValueGetter = memberGetter;
        }
    }
}
