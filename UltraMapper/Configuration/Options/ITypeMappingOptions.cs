namespace UltraMapper
{
    public interface ITypeMappingOptions : IMappingOptions
    {
        bool? IgnoreMemberMappingResolvedByConvention { get; set; }
    }
}
