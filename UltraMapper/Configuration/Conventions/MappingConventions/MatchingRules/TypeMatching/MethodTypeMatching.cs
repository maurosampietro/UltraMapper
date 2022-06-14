using System;
using System.Globalization;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MethodTypeMatching : ITypeMatchingRule
    {
        public bool CanHandle( MemberInfo source, MemberInfo target )
        {
            return source is MethodInfo && target is MethodInfo;
        }

        public bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            if( source is MethodInfo smi && !smi.IsGetterMethod() )
                return false;

            if( target is MethodInfo tmi && !tmi.IsSetterMethod() )
                return false;

            return source.GetMemberType() == target.GetMemberType();
        }
    }
}
