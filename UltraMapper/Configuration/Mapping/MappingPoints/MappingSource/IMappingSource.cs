using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public interface IMappingSource : IMappingPoint
    {
        LambdaExpression ValueGetter { get; }
    }
}
