using System.Collections.Generic;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class QueueMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            var memberType = mapping.TargetMember.MemberInfo.GetMemberType();
            return base.CanHandle( mapping ) && memberType.IsGenericType
                && memberType.GetGenericTypeDefinition() == typeof( Queue<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            //Queue<int> is used only because it is forbidden to use nameof with open generics.
            //Any other type instead of int would work.
            var methodName = nameof( Queue<int>.Enqueue );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetMemberType.GetMethod( methodName, methodParams );
        }
    }
}
