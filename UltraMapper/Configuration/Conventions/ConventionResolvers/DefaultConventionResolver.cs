using System.Collections.Generic;
using System.Linq;
using UltraMapper.Conventions;
using UltraMapper.Conventions.Resolvers;
using UltraMapper.Internals;

namespace UltraMapper
{
    public class DefaultConventionResolver : IConventionResolver
    {
        public void MapByConvention( TypeMapping typeMapping, IEnumerable<IMappingConvention> conventions )
        {
            foreach( var convention in conventions )
            {
                var memberPairings = convention.MapByConvention(
                    typeMapping.SourceType, typeMapping.TargetType );

                foreach( var memberPair in memberPairings )
                {
                    var sourceMember = memberPair.SourceMemberPath.Last();
                    var mappingSource = typeMapping.GetMappingSource( sourceMember, memberPair.SourceMemberPath );

                    var targetMember = memberPair.TargetMemberPath.Last();
                    var mappingTarget = typeMapping.GetMappingTarget( targetMember, memberPair.TargetMemberPath );

                    var mapping = new MemberMapping( typeMapping, mappingSource, mappingTarget )
                    {
                        MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION
                    };

                    typeMapping.MemberMappings[ mappingTarget ] = mapping;
                }
            }
        }
    }
}
