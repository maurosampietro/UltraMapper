using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.CollectionMappingStrategies;

namespace TypeMapper.Internals
{
    public class MappingTarget : MappingMemberBase
    {
        public LambdaExpression ValueSetter { get; set; }
        public LambdaExpression ValueGetter { get; set; }

        public LambdaExpression CustomConstructor { get; set; }
        public ICollectionMappingStrategy CollectionStrategy { get; set; }

        internal MappingTarget( LambdaExpression memberGetter, LambdaExpression memberSetter )
            : base( memberGetter.ExtractMember() )
        {
            //this.CollectionStrategy = new NewCollection();

            this.ValueGetter = memberGetter;
            this.ValueSetter = memberSetter;
        }
    }
}
