using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public class ConditionalMappingSource:MappingPoint, IMappingSource
    {
        private readonly LambdaExpression _conditionalSourceGetter;

        public ConditionalMappingSource(
            LambdaExpression sourceMemberGetterExpression,
            LambdaExpression conditionalSourceGetter ) : base( sourceMemberGetterExpression.GetMemberAccessPath() )
        {
            _conditionalSourceGetter = conditionalSourceGetter;
        }

        public LambdaExpression ValueGetter => _conditionalSourceGetter;   
    }
}
