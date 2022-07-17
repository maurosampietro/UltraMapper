using System;
using System.Collections.Generic;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Internals.ExtensionMethods;

namespace UltraMapper.MappingExpressionBuilders
{
    public class QueueMapper : CollectionMapper
    {
        public QueueMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return base.CanHandle( mapping ) &&
                target.EntryType.IsCollectionOfType( typeof( Queue<> ) );
        }

        protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            var methodName = nameof( Queue<int>.Enqueue );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetInstance.Type.GetMethod( methodName, methodParams );
        }

        protected override MethodInfo GetUpdateCollectionMethod( CollectionMapperContext context )
        {
            return typeof( LinqExtensions ).GetMethod
               (
                   nameof( LinqExtensions.UpdateQueue ),
                   BindingFlags.Static | BindingFlags.Public
               )
               .MakeGenericMethod
               (
                   context.SourceCollectionElementType,
                   context.TargetCollectionElementType
               );
        }
    }
}