using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public interface IMapperExpressionBuilder
    {
        /// <summary>
        /// Gets a value indicating whether the mapper can handle <paramref name="mapping"/>
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns>True if the mapping can be handled by the mapper, False otherwise.</returns>
        bool CanHandle( Type source, Type target );
    }

    public interface ITypeMappingMapperExpression : IMapperExpressionBuilder
    {
        /// <summary>
        /// Gets an expression that can handle <paramref name="mapping"/>
        /// </summary>
        /// <param name="mapping">the property mapping to handle</param>
        /// <returns>Returns a list of objects that need to be recursively mapped</returns>
        LambdaExpression GetMappingExpression( Type source, Type target );
    }

    public interface IMemberMappingMapperExpression : IMapperExpressionBuilder
    {
        /// <summary>
        /// Gets an expression that can handle <paramref name="mapping"/>
        /// </summary>
        /// <param name="mapping">the property mapping to handle</param>
        /// <returns>Returns a list of objects that need to be recursively mapped</returns>
        LambdaExpression GetMappingExpression( MemberMapping mapping );
    }
}
