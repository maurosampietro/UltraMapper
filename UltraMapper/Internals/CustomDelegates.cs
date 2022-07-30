using System;
using System.Collections.Generic;
using System.Text;

namespace UltraMapper.Internals
{
    //public delegate void UltraMapperAction<TSource,TTarget>( ReferenceTracker reference, TSource source, ref TTarget target );
    //public delegate void UltraMapperAction( ReferenceTracker reference, object source, ref object target );

    public delegate TTarget UltraMapperFunc<TSource, TTarget>( ReferenceTracker reference, TSource source, TTarget target );
    public delegate object UltraMapperFunc( ReferenceTracker reference, object source, object target );

    //public delegate TTarget UltraMapperFuncPrimitives<TSource,TTarget>( ReferenceTracker reference, TSource source );
    //public delegate object UltraMapperFuncPrimitives( ReferenceTracker reference, object source );
}
