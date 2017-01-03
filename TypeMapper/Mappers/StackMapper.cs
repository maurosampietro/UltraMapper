using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class StackMapper : CollectionMapper
    {
        public override bool CanHandle( PropertyMapping mapping )
        {
            return mapping.SourceProperty.PropertyInfo.PropertyType
                .GetGenericTypeDefinition() == typeof( Stack<> );
        }

        /// <summary>
        /// Gets an expression that represent the construction of the target collection
        /// from a source collection. This constructor is used when mapping to value types, or built-in types.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression GetTargetCollectionConstructorFromCollectionExpression( CollectionMapperContext context )
        {
            var constructor = GetTargetCollectionConstructorFromCollection( context );
            //To create an identical Stack preserving order we need to read a Stack twice
            return Expression.New( constructor, Expression.New( constructor, context.SourceCollection ) );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetCollectionType.GetMethod( "Push" );
        }
    }

    public class QueueMapper : CollectionMapper
    {
        public override bool CanHandle( PropertyMapping mapping )
        {
            return mapping.SourceProperty.PropertyInfo.PropertyType
                .GetGenericTypeDefinition() == typeof( Queue<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetCollectionType.GetMethod( "Enqueue" );
        }
    }

    public class LinkedListMapper : CollectionMapper
    {
        public override bool CanHandle( PropertyMapping mapping )
        {
            return mapping.SourceProperty.PropertyInfo.PropertyType
                .GetGenericTypeDefinition() == typeof( LinkedList<> );
        }

        protected override MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetCollectionType.GetMethod( 
                "AddLast", new[] { context.TargetElementType } );
        }
    }
}
