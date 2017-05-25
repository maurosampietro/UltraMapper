using System;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

namespace UltraMapper.MappingExpressionBuilders
{
    public interface IMappingExpressionBuilder
    {
        /// <summary>
        /// Gets a value indicating whether the mapper can handle the
        /// mapping between <paramref name="source"/> and <paramref name="target"/>
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="target">Target type</param>
        /// <returns>True if the mapping can be handled by the mapper, False otherwise.</returns>
        bool CanHandle( Type source, Type target );

        /// <summary>
        /// Gets an expression capable of mapping between 
        /// <paramref name="source"/> and <paramref name="target"/>
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="target">Target type</param>
        /// <param name="options">Mapping options</param>
        /// <returns>The mapping expression</returns>
        LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options );
    }

    internal interface IMemberMappingExpression
    {
        Expression GetMemberAssignment( MemberMappingContext memberContext );
    }
}
