using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ReadOnlyCollectionMapper : CollectionMappingViaTemporaryCollection
    {
        public ReadOnlyCollectionMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return base.CanHandle( source, target ) &&
               target.ImplementsInterface( typeof( IReadOnlyCollection<> ) );
        }

        //protected internal override Expression GetNewTargetInstanceExpression( MemberMappingContext context )
        //{
        //    var constructor = context.TargetMember.Type
        //        .GetConstructor( new Type[] { typeof( IList<> ).MakeGenericType( context.SourceCollectionElementType ) );

        //    return Expression.New( constructor, context.SourceMember );
        //}
    }
}
