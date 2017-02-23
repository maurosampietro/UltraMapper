using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public interface IMapperExpression
    {
        /// <summary>
        /// Gets a value indicating whether the mapper can handle
        /// the mapping from <param name="sourceType">source</param>
        /// to <param name="targetType">target</param>.
        /// </summary>
        /// <param name="sourceType">source type</param>
        /// <param name="targetType">target type</param>
        /// <returns>True if this mapper can handle the mapping from <param name="sourceType">source</param>
        /// to <param name="targetType">target</param>, False otherwise.</returns>
        bool CanHandle( Type sourceType, Type targetType );

        /// <summary>
        /// Gets the expression to map from <param name="sourceType">source</param>
        /// to <param name="targetType">target</param>.
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns>The mapping expression</returns>
        LambdaExpression GetMappingExpression( Type sourceType, Type targetType );
    }

    public interface IObjectMapperExpression 
    {
        /// <summary>
        /// Gets a value indicating whether the mapper can handle <paramref name="mapping"/>
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns>True if the mapping can be handled by the mapper, False otherwise.</returns>
        bool CanHandle( MemberMapping mapping );

        /// <summary>
        /// Gets an expression that can handle <paramref name="mapping"/>
        /// </summary>
        /// <param name="mapping">the property mapping to handle</param>
        /// <returns>Returns a list of objects that need to be recursively mapped</returns>
        LambdaExpression GetMappingExpression( MemberMapping mapping );
    }
}
