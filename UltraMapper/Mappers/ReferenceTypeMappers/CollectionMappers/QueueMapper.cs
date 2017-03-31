using System;
using System.Collections.Generic;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Mappers
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
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            var methodName = nameof( Queue<int>.Enqueue );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetInstance.Type.GetMethod( methodName, methodParams );
        }
    }
}
