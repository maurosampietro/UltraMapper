using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public interface IMappingTarget : IMappingPoint
    {
        LambdaExpression ValueGetter { get; }
        LambdaExpression ValueSetter { get; }
        LambdaExpression CustomConstructor { get; }
    }
}