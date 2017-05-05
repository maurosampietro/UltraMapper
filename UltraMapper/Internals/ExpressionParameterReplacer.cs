using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    internal class ExpressionParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly string _name;

        protected override Expression VisitParameter( ParameterExpression node )
        {
            if( node.Name == _name && (node.Type == _parameter.Type ||
                node.Type.IsAssignableFrom( _parameter.Type )) )
            {
                return base.VisitParameter( _parameter );
            }

            return base.VisitParameter( node );
        }

        internal ExpressionParameterReplacer( ParameterExpression parameter, string name )
        {
            _parameter = parameter;
            _name = name;
        }
    }
}
