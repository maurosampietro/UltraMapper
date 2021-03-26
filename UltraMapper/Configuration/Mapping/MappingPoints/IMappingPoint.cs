using System;
using System.Collections.Generic;
using System.Text;

namespace UltraMapper.Internals
{
    public interface IMappingPoint
    {
        MemberAccessPath MemberAccessPath { get; }
        bool Ignore { get; set; }
    }
}
