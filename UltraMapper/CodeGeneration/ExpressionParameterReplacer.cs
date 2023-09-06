using System;
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    internal class ExpressionParameterReplacer : ExpressionVisitor
    {
        private readonly Expression _expression;
        private readonly string _name;

        protected override Expression VisitParameter( ParameterExpression node )
        {
            if( node.Name == _name && (node.Type == _expression.Type ||
                node.Type.IsAssignableFrom( _expression.Type ) ||
                _expression.Type.IsAssignableFrom( node.Type )) )
            {
                return _expression;
            }

            return base.VisitParameter( node );
        }

        internal ExpressionParameterReplacer( Expression expression, string name )
        {
            _expression = expression;
            _name = name;
        }

        /// <summary>
        /// MemberAccessPathExpressionBuilder.GetGetterExpWithNullChecks changes the expression to Nullable<ValueType>
        /// if the return type is a value type. As a consequence the following override is needed.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitUnary( UnaryExpression node )
        {
            // Check if the operand is a parameter and matches the name
            if( node.NodeType == ExpressionType.Convert && node.Operand is ParameterExpression param && param.Name == _name )
            {
                // Handle the replacement
                if( node.Type == _expression.Type ||
                    node.Type.IsAssignableFrom( _expression.Type ) ||
                    _expression.Type.IsAssignableFrom( node.Type ) )
                {
                    return Expression.Convert( _expression, node.Type );
                }
            }

            // Call the base method for any other scenario
            return base.VisitUnary( node );
        }
    }
}
