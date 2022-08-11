using System;
using System.Collections.Generic;
using System.Text;

namespace UltraMapper.Internals
{
    public delegate TTarget UltraMapperDelegate<TSource, TTarget>( ReferenceTracker reference, TSource source, TTarget target );
    public delegate object UltraMapperDelegate( ReferenceTracker reference, object source, object target );
}
