using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    public class MappingSource : MappingMemberBase
    {
        public LambdaExpression ValueGetter { get; set; }

        public MappingSource( MemberInfo memberInfo )
            : this( new MemberAccessPath( memberInfo ) ) { }

        public MappingSource( MemberAccessPath memberGetter )
            : base( memberGetter )
        {
            this.ValueGetter = memberGetter.GetGetterLambdaExpression();
        }

        public MappingSource( LambdaExpression memberGetter )
            : base( memberGetter.GetMemberAccessPath() )
        {
            this.ValueGetter = this.MemberAccessPath.Count == 1 ? memberGetter :
                this.MemberAccessPath.GetGetterLambdaExpressionWithNullChecks();
        }
    }
}
