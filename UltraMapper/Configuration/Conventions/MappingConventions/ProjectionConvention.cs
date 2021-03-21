using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class ProjectionConvention : IMappingConvention
    {
        public IMemberProvider SourceMemberProvider { get; set; }
        public IMemberProvider TargetMemberProvider { get; set; }

        public IMatchingRulesEvaluator MatchingRulesEvaluator { get; set; }
        public MatchingRules MatchingRules { get; set; }
        public StringSplitter StringSplitter { get; set; }

        public ProjectionConvention()
        {
            this.SourceMemberProvider = new SourceMemberProvider();
            this.TargetMemberProvider = new TargetMemberProvider();
            this.StringSplitter = new StringSplitter( StringSplittingRules.PascalCase );
            this.MatchingRules = new MatchingRules();
            this.MatchingRulesEvaluator = new DefaultMatchingRuleEvaluator();
        }

        public IEnumerable<MemberPair> MapByConvention( Type source, Type target )
        {
            //unflattening: CustomerName -> Customer.Name
            //unflattening: GetCustomerName() -> Customer.Name
            //unflattening: GetCustomerName() -> GetCustomer().SetName();
            var sourceMembers = this.SourceMemberProvider.GetMembers( source ).ToList();
            foreach( var sourceMember in sourceMembers )
            {
                var splitNames = this.StringSplitter.Split( sourceMember.Name ).ToList();
                this.ClearMethodPrefixes( sourceMember, splitNames );
                if( splitNames.Count <= 1 ) continue;

                var targetAccessPath = FollowPathUnflattening( target, splitNames, this.TargetMemberProvider );
                if( targetAccessPath != null && this.MatchingRulesEvaluator.IsMatch( sourceMember, targetAccessPath.Last(), this.MatchingRules ) )
                    yield return new MemberPair( sourceMember, targetAccessPath );
            }

            //flattening: Customer.Name -> CustomerName
            //flattening: Customer.Name -> SetCustomerName(string)
            //flattening: GetCustomer().GetName() -> CustomerName
            var targetMembers = this.TargetMemberProvider.GetMembers( target ).ToList();
            foreach( var targetMember in targetMembers )
            {
                var splitNames = this.StringSplitter.Split( targetMember.Name ).ToList();
                this.ClearMethodPrefixes( targetMember, splitNames );
                if( splitNames.Count <= 1 ) continue;

                var sourceAccessPath = FollowPathFlattening( source, splitNames, this.SourceMemberProvider );

                if( sourceAccessPath != null && this.MatchingRulesEvaluator.IsMatch( sourceAccessPath.Last(), targetMember, this.MatchingRules ) )
                    yield return new MemberPair( sourceAccessPath, targetMember );
            }
        }

        private MemberAccessPath FollowPathFlattening( Type type, IEnumerable<string> memberNames, IMemberProvider memberProvider )
        {
            var accessPath = new MemberAccessPath();

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !memberProvider.IgnoreNonPublicMembers )
                bindingAttributes |= BindingFlags.NonPublic;

            foreach( var splitName in memberNames )
            {
                var members = type.GetMember( splitName, bindingAttributes );
                if( members.Length == 0 )
                {
                    var getMethodPrefixes = new string[] { "Get_", "Get", "get", "get_" };
                    foreach( var prefix in getMethodPrefixes )
                    {
                        members = type.GetMember( prefix + splitName, bindingAttributes );
                        if( members.Length > 0 ) break;
                    }

                    if( members.Length == 0 )
                        return null;
                }

                var sourceMember = members?[ 0 ];
                type = sourceMember.GetMemberType();
                accessPath.Add( sourceMember );
            }

            return accessPath;
        }

        private MemberAccessPath FollowPathUnflattening( Type type, IEnumerable<string> memberNames, IMemberProvider memberProvider )
        {
            var accessPath = new MemberAccessPath();

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !memberProvider.IgnoreNonPublicMembers )
                bindingAttributes |= BindingFlags.NonPublic;

            foreach( var splitName in memberNames.Take( memberNames.Count() - 1 ) )
            {
                var members = type.GetMember( splitName, bindingAttributes );
                if( members.Length == 0 )
                {
                    var getMethodPrefixes = new string[] { "Get_", "Get", "get", "get_" };
                    foreach( var prefix in getMethodPrefixes )
                    {
                        members = type.GetMember( prefix + splitName, bindingAttributes );
                        if( members.Length > 0 ) break;
                    }

                    if( members.Length == 0 )
                        return null;
                }

                var sourceMember = members?[ 0 ];
                type = sourceMember.GetMemberType();
                accessPath.Add( sourceMember );
            }

            {
                var members = type.GetMember( memberNames.Last(), bindingAttributes );
                if( members.Length == 0 )
                {
                    var getMethodPrefixes = new string[] { "Set_", "Set", "set", "set_" };
                    foreach( var prefix in getMethodPrefixes )
                    {
                        members = type.GetMember( prefix + memberNames.Last(), bindingAttributes );
                        if( members.Length > 0 ) break;
                    }

                    if( members.Length == 0 )
                        return null;
                }

                var sourceMember = members?[ 0 ];
                type = sourceMember.GetMemberType();
                accessPath.Add( sourceMember );
            }

            return accessPath;
        }

        private void ClearMethodPrefixes( MemberInfo sourceMember, List<string> splitNames )
        {
            if( sourceMember is MethodInfo )
            {
                var getMethodPrefixes = new string[] { "Get_", "Get" };
                var setMethodPrefixes = new string[] { "Set_", "Set" };
                var prefixes = getMethodPrefixes.Concat( setMethodPrefixes );

                if( prefixes.Any( gmp => splitNames[ 0 ].Equals( gmp,
                    StringComparison.InvariantCultureIgnoreCase ) ) )
                {
                    splitNames.RemoveAt( 0 );
                }
            }
        }
    }
}
