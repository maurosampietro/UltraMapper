using System;

namespace UltraMapper.Internals
{
    public interface IMappingPoint
    {
        Type EntryType { get; }
        Type ReturnType { get; }

        Type MemberType { get; }
        MemberAccessPath MemberAccessPath { get; }
        bool Ignore { get; set; }
    }
}
