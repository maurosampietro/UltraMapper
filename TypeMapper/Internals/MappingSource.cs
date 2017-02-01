using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class MappingSource : MappingMemberBase
    {
        public LambdaExpression ValueGetter { get; set; }

        internal MappingSource( MemberInfo memberInfo )
            : base( memberInfo )
        {
            //((MemberExpression)propertySelector.Body).Member
            this.ValueGetter = memberInfo.GetGetterLambdaExpression();
        }
    }
}
