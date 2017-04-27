using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Internals
{
    public class MappingTarget : MappingMemberBase
    {
        public LambdaExpression ValueSetter { get; set; }
        public LambdaExpression ValueGetter { get; set; }

        public LambdaExpression CustomConstructor { get; set; }

        internal MappingTarget( LambdaExpression memberGetter, LambdaExpression memberSetter )
            : base( memberSetter.ExtractMember() )
        {
            this.ValueGetter = memberGetter;
            this.ValueSetter = memberSetter;
        }
    }
}
