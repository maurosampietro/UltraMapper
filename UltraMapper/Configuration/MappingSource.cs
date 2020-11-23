using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public class MappingSource : MappingMemberBase
    {
        public LambdaExpression ValueGetter { get; set; }

        public MappingSource( MemberAccessPath memberGetter )
            : base( memberGetter )
        {
            this.ValueGetter = memberGetter.GetGetterLambdaExpression();
        }

        public MappingSource( LambdaExpression memberGetter )
            : base( memberGetter.ExtractMember() )
        {
            this.ValueGetter = this.MemberAccessPath.Count == 1 ? memberGetter :
                this.MemberAccessPath.GetGetterLambdaExpressionWithNullChecks();
        }
    }
}
