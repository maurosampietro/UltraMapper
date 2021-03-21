using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    public class MappingTarget : MappingPoint, IMappingTarget
    {
        public LambdaExpression ValueGetter { get; }
        public LambdaExpression ValueSetter { get; }

        public LambdaExpression CustomConstructor { get; set; }

        public MappingTarget( MemberInfo memberInfo )
            : this( new MemberAccessPath( memberInfo ), null ) { }

        public MappingTarget( MemberAccessPath memberSetter, MemberAccessPath memberGetter = null )
            : base( memberSetter )
        {
            this.ValueSetter = memberSetter.Count > 1 ?
                memberSetter.GetSetterExpWithNullInstancesInstantiation() :
                memberSetter.GetSetterExp();

            try
            {
                //build the getter from the getter member path if provided;
                //try to figure out the getter from the setter member path otherwise
                //(this will work if the member being accessed is a field or property
                //but won't necessarily work for methods)
                this.ValueGetter = memberGetter == null
                    ? memberSetter.GetGetterExp()
                    : memberGetter.GetGetterExp();
            }
            catch( Exception )
            {
                //Must be provided from where to read the member.
                //We don't always have the real need to 'read' the member being set (we need to write it).
                //This could still be not a problem.
            }
        }

        public MappingTarget( LambdaExpression memberSetter, LambdaExpression memberGetter = null )
            : base( memberSetter.GetMemberAccessPath() )
        {
            this.ValueGetter = memberGetter?.GetMemberAccessPath()
                .GetGetterExpWithNullChecks();

            this.ValueSetter = this.MemberAccessPath.Count == 1 ? memberSetter :
                this.MemberAccessPath.GetSetterExpWithNullChecks();
        }
    }

    public class MappingTarget<TSource, TTarget> : MappingPoint, IMappingTarget
    {
        public LambdaExpression ValueGetter { get; }
        public LambdaExpression ValueSetter { get; }

        public LambdaExpression CustomConstructor { get; set; }

        public MappingTarget( Expression<Func<TSource, TTarget>> memberSetter, 
            Expression<Func<TSource, TTarget>> memberGetter = null )
            :base( memberSetter.GetMemberAccessPath() )
        {
            this.ValueGetter = memberGetter?.GetMemberAccessPath()
                .GetGetterExpWithNullChecks();

            this.ValueSetter = this.MemberAccessPath.Count == 1 ? memberSetter :
                this.MemberAccessPath.GetSetterExpWithNullChecks();
        }
    }
}
