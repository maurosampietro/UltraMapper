using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;
using UltraMapper.Conventions;
using System.Collections.ObjectModel;

namespace UltraMapper.Conventions
{
    public class ConventionResolver : IConventionResolver
    {
        public readonly IMappingConvention MappingConvention;

        public IMemberProvider SourceMemberProvider { get; }
        public IMemberProvider TargetMemberProvider { get; }

        public ConventionResolver( IMappingConvention mappingConvention )
            : this( mappingConvention, new SourceMemberProvider(), new TargetMemberProvider() ) { }

        public ConventionResolver( IMappingConvention mappingConvention,
            IMemberProvider sourceMemberProvider, IMemberProvider targetMembetProvider )
        {
            this.MappingConvention = mappingConvention;
            this.SourceMemberProvider = sourceMemberProvider;
            this.TargetMemberProvider = targetMembetProvider;
        }

        public IEnumerable<MemberPair> Resolve( Type source, Type target )
        {
            var sourceMembers = this.SourceMemberProvider.GetMembers( source );
            var targetMembers = this.TargetMemberProvider.GetMembers( target ).ToList();

            foreach( var sourceMember in sourceMembers )
            {
                foreach( var targetMember in targetMembers )
                {
                    if( this.MappingConvention.IsMatch( sourceMember, targetMember ) )
                    {
                        yield return new MemberPair( sourceMember, targetMember );
                        break; //sourceMember is now mapped, jump directly to the next sourceMember
                    }
                }
            }
        }
    }
}
