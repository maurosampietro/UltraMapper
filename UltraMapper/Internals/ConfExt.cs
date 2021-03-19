using System.Collections.Generic;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper
{
    public static class ConfigExt
    {
        public static void InsertRangeAfter<TMappingExpressionBuilder>(
            this List<IMappingExpressionBuilder> list, params IMappingExpressionBuilder[] mebs )
        {
            int index = list.FindIndex( m => m is ReferenceMapper );
            list.InsertRange( index, mebs );
        }
    }
}
