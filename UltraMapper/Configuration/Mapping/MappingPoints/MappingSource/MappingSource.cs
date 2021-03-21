using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    public class MappingSource : MappingPoint, IMappingSource
    {
        public LambdaExpression ValueGetter { get; }

        public MappingSource( MemberInfo memberInfo )
            : this( new MemberAccessPath( memberInfo ) ) { }

        public MappingSource( MemberAccessPath memberGetter )
            : base( memberGetter )
        {
            this.ValueGetter = this.MemberAccessPath.Count == 1 ?
                this.MemberAccessPath.GetGetterExp() :
                this.MemberAccessPath.GetGetterExpWithNullChecks();
        }

        public MappingSource( LambdaExpression memberGetter )
            : base( memberGetter.GetMemberAccessPath() )
        {
            this.ValueGetter = this.MemberAccessPath.Count == 1 ? memberGetter :
                this.MemberAccessPath.GetGetterExpWithNullChecks();
        }

        public MappingSource( Expression memberGetter )
           : base( memberGetter.GetMemberAccessPath() )
        {
            this.ValueGetter = this.MemberAccessPath.Count == 1 ?
                this.MemberAccessPath.GetGetterExp() :
                this.MemberAccessPath.GetGetterExpWithNullChecks();
        }
    }

    public class MappingSource<TSource, TTarget> : MappingPoint, IMappingSource
    {
        public LambdaExpression ValueGetter { get; }

        public MappingSource( Expression<Func<TSource, TTarget>> memberGetter )
            : base( memberGetter.GetMemberAccessPath() )
        {
            this.ValueGetter = this.MemberAccessPath.Count == 1 ? memberGetter :
                this.MemberAccessPath.GetGetterExpWithNullChecks();
        }
    }
}
