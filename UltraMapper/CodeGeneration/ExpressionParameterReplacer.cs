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
    }
}
