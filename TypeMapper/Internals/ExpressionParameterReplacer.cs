using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    internal class ExpressionParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly string _name;

        protected override Expression VisitParameter( ParameterExpression node )
        {
            if( _name == null && node.Type == _parameter.Type )
                return base.VisitParameter( _parameter );

            if( node.Type == _parameter.Type && node.Name == _name )
                return base.VisitParameter( _parameter );

            return base.VisitParameter( node );
        }

        internal ExpressionParameterReplacer( ParameterExpression parameter, string name = null )
        {
            _parameter = parameter;
            _name = name;
        }
    }
}
