using System.Linq.Expressions;

namespace TypeMapper.Internals
{
    internal class ExpressionParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly string _name;

        protected override Expression VisitParameter( ParameterExpression node )
        {
            if( node.Type == _parameter.Type && node.Name == _name )
                return base.VisitParameter( _parameter );

            return base.VisitParameter( node );
        }

        internal ExpressionParameterReplacer( ParameterExpression parameter, string name )
        {
            _parameter = parameter;
            _name = name;
        }
    }
}
