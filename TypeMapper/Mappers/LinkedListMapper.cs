using System.Collections.Generic;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class LinkedListMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            var memberType = mapping.TargetProperty.MemberInfo.GetMemberType();
            return base.CanHandle( mapping ) && memberType.IsGenericType
                && memberType.GetGenericTypeDefinition() == typeof( LinkedList<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            //LinkedList<int> is used only because it is forbidden to use nameof with open generics.
            //Any other type instead of int would work.
            var methodName = nameof( LinkedList<int>.AddLast );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetMemberType.GetMethod( methodName, methodParams );
        }
    }
}
