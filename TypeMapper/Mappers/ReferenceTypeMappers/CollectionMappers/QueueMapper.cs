using System;
using System.Collections.Generic;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class QueueMapper : CollectionMapper
    {
        public QueueMapper( MapperConfiguration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) && target.IsGenericType
                && target.GetGenericTypeDefinition() == typeof( Queue<> );
        }

        protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //Queue<int> is used only because it is forbidden to use nameof with unbound generic types.
            //Any other type instead of int would work.
            var methodName = nameof( Queue<int>.Enqueue );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetInstance.Type.GetMethod( methodName, methodParams );
        }
    }
}
