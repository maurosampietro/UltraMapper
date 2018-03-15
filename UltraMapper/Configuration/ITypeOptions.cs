namespace UltraMapper
{
    public interface ITypeOptions : IMappingOptions
    {
        bool IgnoreMemberMappingResolvedByConvention { get; set; }
    }
}
