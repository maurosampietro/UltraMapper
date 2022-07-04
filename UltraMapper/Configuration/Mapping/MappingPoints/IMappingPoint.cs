using System;

namespace UltraMapper.Internals
{
    public interface IMappingPoint
    {
        Type MemberType { get; }
        MemberAccessPath MemberAccessPath { get; }
        bool Ignore { get; set; }
    }
}
