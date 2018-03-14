using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

namespace UltraMapper.MappingExpressionBuilders
{
    /*Stack<T> and other LIFO collections require the list to be read in reverse  
    * to preserve order and have a specular clone */
    public class StackMapper : CollectionMappingViaTemporaryCollection
    {
        public StackMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) &&
                target.IsCollectionOfType( typeof( Stack<> ) );
        }

        protected override MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            var methodName = nameof( Stack<int>.Push );
            var methodParams = new[] { context.TargetCollectionElementType };

            return context.TargetInstance.Type.GetMethod( methodName, methodParams );
        }

        protected override Type GetTemporaryCollectionType( CollectionMapperContext context )
        {
            return typeof( Stack<> ).MakeGenericType( context.SourceCollectionElementType );
        }

        protected override Expression GetNewInstanceFromSourceCollection( MemberMappingContext context, CollectionMapperContext collectionContext )
        {
            var targetConstructor = context.TargetMember.Type.GetConstructor(
               new[] { typeof( IEnumerable<> ).MakeGenericType( collectionContext.TargetCollectionElementType ) } );

            return Expression.New( targetConstructor, 
                Expression.New( targetConstructor, context.SourceMember ) );
        }
    }
}