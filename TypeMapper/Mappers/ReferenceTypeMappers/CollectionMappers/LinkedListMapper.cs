using System;
using System.Collections.Generic;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class LinkedListMapper : CollectionMapper
    {
        public LinkedListMapper( MapperConfiguration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) && target.IsGenericType
                && target.GetGenericTypeDefinition() == typeof( LinkedList<> );
        }

        protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //LinkedList<int> is used only because it is forbidden to use nameof with unbound generic types.
            //Any other type instead of int would work.
            var methodName = nameof( LinkedList<int>.AddLast );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetInstance.Type.GetMethod( methodName, methodParams );
        }
    }
}
