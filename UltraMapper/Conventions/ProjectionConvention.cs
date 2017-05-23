using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class ProjectionConvention : IMappingConvention
    {
        public IMemberProvider SourceMemberProvider { get; set; }
        public IMemberProvider TargetMemberProvider { get; set; }
        public IMatchingRuleEvaluator MatchingRuleEvaluator { get; set; }
        public MatchingRules MatchingRules { get; set; }
        public StringSplitter Stringsplitter { get; set; }

        public ProjectionConvention()
        {
            this.SourceMemberProvider = new SourceMemberProvider();
            this.TargetMemberProvider = new TargetMemberProvider();
            this.Stringsplitter = new StringSplitter( StringSplittingRules.PascalCase );
            this.MatchingRules = new MatchingRules();
        }

        public IEnumerable<MemberPair> MapByConvention( Type source, Type target )
        {
            if( MatchingRuleEvaluator == null )
                MatchingRuleEvaluator = new DefaultMatchingRuleEvaluator( this.MatchingRules );

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.TargetMemberProvider.IgnoreNonPublicMembers )
                bindingAttributes |= BindingFlags.NonPublic;

            var targetMembers = this.TargetMemberProvider.GetMembers( target ).ToList();
            foreach( var targetMember in targetMembers )
            {
                var sourceAccessPath = new MemberAccessPath();

                Type sourceType = source;
                MemberInfo sourceMember = null;

                bool match = true;
                var splitNames = this.Stringsplitter.Split( targetMember.Name );
                foreach( var splitName in splitNames )
                {
                    var members = sourceType.GetMember( splitName, bindingAttributes );

                    if( members == null || members.Length == 0 )
                    {
                        match = false;
                        break;
                    }
                    else
                    {
                        sourceMember = members?[ 0 ];
                        if( sourceMember is MethodInfo )
                        {
                            var methodInfo = sourceMember as MethodInfo;
                            if( !methodInfo.IsGetterMethod() )
                            {
                                match = false;
                                break;
                            }
                        }

                        sourceType = sourceMember.GetMemberType();
                        sourceAccessPath.Add( sourceMember );
                    }
                }

                if( match && this.MatchingRuleEvaluator.IsMatch( sourceAccessPath.Last(), targetMember ) )
                    yield return new MemberPair( sourceAccessPath, targetMember );
            }
        }
    }
}
