using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MapperContext
    {
        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public IMappingOptions Options { get; protected set; }

        public MapperContext( Type source, Type target, IMappingOptions options )
        {
            SourceInstance = Expression.Parameter( source, "sourceInstance" );
            TargetInstance = Expression.Parameter( target, "targetInstance" );

            switch( options )
            {
                case MemberMappingOptionsInheritanceTraversal mmc: this.Options = mmc; break;
                case TypeMappingOptionsInheritanceTraversal tmc: this.Options = tmc; break;
                case IMemberMappingOptions _: this.Options = new MemberMappingOptionsInheritanceTraversal( (MemberMapping)options ); break;
                case ITypeMappingOptions _: this.Options = new TypeMappingOptionsInheritanceTraversal( (TypeMapping)options ); break;
            }
        }
    }
}
