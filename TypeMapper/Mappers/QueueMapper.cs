using System.Collections.Generic;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class QueueMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            var memberType = mapping.TargetProperty.MemberInfo.GetMemberType();
            return base.CanHandle( mapping ) && memberType.IsGenericType
                && memberType.GetGenericTypeDefinition() == typeof( Queue<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetPropertyType.GetMethod(
                "Enqueue", new[] { context.TargetElementType } );
        }
    }
}
