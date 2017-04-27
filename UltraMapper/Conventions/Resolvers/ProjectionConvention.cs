using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions.Resolvers
{
    public class ProjectionConvention : IConventionResolver
    {
        private StringSplitter _splitter;

        public IMemberProvider SourceMemberProvider { get; set; }
        public IMemberProvider TargetMemberProvider { get; set; }
        public IStringSplittingRule SplittingRule { get; private set; }

        public ProjectionConvention()
            : this( new SourceMemberProvider(), new TargetMemberProvider(), new PascalCaseStringSplittingRule() ) { }

        public ProjectionConvention( IMemberProvider sourceMemberProvider,
            IMemberProvider targetMembetProvider, IStringSplittingRule splittingRule )
        {
            this.SourceMemberProvider = sourceMemberProvider;
            this.TargetMemberProvider = targetMembetProvider;
            this.SplittingRule = splittingRule;

            _splitter = new StringSplitter( splittingRule );
        }

        public IEnumerable<MemberPair> Resolve( Type source, Type target )
        {
            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.TargetMemberProvider.IgnoreNonPublicMembers )
                bindingAttributes |= BindingFlags.NonPublic;

            var sourceMembers = this.SourceMemberProvider.GetMembers( source );
            var targetMembers = this.TargetMemberProvider.GetMembers( target ).ToList();

            foreach( var targetMember in targetMembers )
            {
                var sourceAccessPath = new MemberAccessPath();

                Type sourceType = source;
                MemberInfo sourceMember = null;
                var splitNames = _splitter.Split( targetMember.Name );

                foreach( var splitName in splitNames )
                {
                    var members = sourceType.GetMember( splitName, bindingAttributes );

                    if( members == null || members.Length == 0 ) break;
                    else
                    {
                        sourceMember = members?[ 0 ];
                        sourceType = sourceMember.GetMemberType();

                        sourceAccessPath.Add( sourceMember );
                    }
                }

                if( sourceMember != null )
                    yield return new MemberPair( sourceAccessPath, targetMember );
            }
        }
    }
}
