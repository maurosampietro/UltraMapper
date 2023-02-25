using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public class ConditionalMappingSource : MappingSource
    {
        private readonly LambdaExpression _conditionalSourceGetter;

        public ConditionalMappingSource(
            LambdaExpression sourceMemberGetterExpression,
            LambdaExpression conditionalSourceGetter ) : base( sourceMemberGetterExpression )
        {
            _conditionalSourceGetter = conditionalSourceGetter;
        }

        public override LambdaExpression ValueGetter => _conditionalSourceGetter;
    }
}