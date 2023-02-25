using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    public class MappingSource : MappingPoint, IMappingSource
    {
        public virtual LambdaExpression ValueGetter { get; }

        public MappingSource( Type type )
            : this( new MemberAccessPath( type ) ) { }

        public MappingSource( MemberInfo memberInfo )
            : this( new MemberAccessPath( memberInfo.DeclaringType ?? (Type)memberInfo, memberInfo ) ) { }

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

    public sealed class MappingSource<TSource, TTarget> : MappingPoint, IMappingSource
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