using System.Collections.Generic;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class QueueMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            return mapping.TargetProperty.MemberInfo.GetMemberType()
                .GetGenericTypeDefinition() == typeof( Queue<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetPropertyType.GetMethod(
                "Enqueue", new[] { context.TargetElementType } );
        }
    }
}
