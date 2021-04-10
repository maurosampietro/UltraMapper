namespace UltraMapper.Internals
{
    public interface IMappingPoint
    {
        MemberAccessPath MemberAccessPath { get; }
        bool Ignore { get; set; }
    }
}
