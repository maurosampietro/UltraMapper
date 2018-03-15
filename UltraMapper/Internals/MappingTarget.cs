using System;
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public class MappingTarget : MappingMemberBase
    {
        public LambdaExpression ValueSetter { get; set; }
        public LambdaExpression ValueGetter { get; set; }

        public LambdaExpression CustomConstructor { get; set; }

        internal MappingTarget( MemberAccessPath memberSetter, MemberAccessPath memberGetter = null )
            : base( memberSetter )
        {
            this.ValueSetter = memberSetter.GetSetterLambdaExpression();

            try
            {
                //build the getter from the getter member path if provided;
                //try to figure out the getter from the setter member path otherwise
                //(this will work for if the member being accessed is a field or property
                //but won't necessarily work for methods)
                this.ValueGetter = memberGetter == null
                    ? memberSetter.GetGetterLambdaExpression()
                    : memberGetter.GetGetterLambdaExpression();
            }
            catch( Exception )
            {
                //Must be provided from where to read the member.
                //We don't always have the real need to 'read' the member being set.
                //This could still be not a problem.
            }
        }

        internal MappingTarget( LambdaExpression memberSetter, LambdaExpression memberGetter = null )
            : base( memberSetter.ExtractMember() )
        {
            this.ValueGetter = memberGetter.ExtractMember()
                .GetGetterLambdaExpressionWithNullChecks();

            this.ValueSetter = this.MemberAccessPath.Count == 1 ? memberSetter :
                this.MemberAccessPath.GetSetterLambdaExpressionWithNullChecks();
        }
    }
}
