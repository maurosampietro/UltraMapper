﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    public interface IMatchingRule
    {
        bool IsCompliant( MemberInfo source, MemberInfo target );
    }
}
