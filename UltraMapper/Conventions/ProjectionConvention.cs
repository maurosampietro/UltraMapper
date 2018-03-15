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
        }

        public IEnumerable<MemberPair> MapByConvention( Type source, Type target )
        {
            if( MatchingRulesEvaluator == null )
                MatchingRulesEvaluator = new DefaultMatchingRuleEvaluator( this.MatchingRules );

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.TargetMemberProvider.IgnoreNonPublicMembers )
                bindingAttributes |= BindingFlags.NonPublic;

            var sourceMembers = this.SourceMemberProvider.GetMembers( source ).ToList();
            foreach( var sourceMember in sourceMembers )
            {
                var targetAccessPath = new MemberAccessPath();

                Type targetType = target;
                MemberInfo targetMember = null;

                bool isMatch = true;
                var splitNames = this.StringSplitter.Split( sourceMember.Name ).ToArray();
                if( splitNames.Length <= 1 ) isMatch = false;

                foreach( var splitName in splitNames )
                {
                    var members = targetType.GetMember( splitName, bindingAttributes );
                    if( members == null || members.Length == 0 )
                    {
                        isMatch = false;
                        break;
                    }

                    targetMember = members?[ 0 ];
                    if( targetMember is MethodInfo )
                    {
                        var methodInfo = targetMember as MethodInfo;
                        if( !methodInfo.IsGetterMethod() )
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    targetType = targetMember.GetMemberType();
                    targetAccessPath.Add( targetMember );
                }

                if( isMatch && this.MatchingRulesEvaluator.IsMatch( sourceMember, targetAccessPath.Last() ) )
                    yield return new MemberPair( sourceMember, targetAccessPath );
            }

            var targetMembers = this.TargetMemberProvider.GetMembers( target ).ToList();
            foreach( var targetMember in targetMembers )
            {
                var sourceAccessPath = new MemberAccessPath();

                Type sourceType = source;
                MemberInfo sourceMember = null;

                bool isMatch = true;
                var splitNames = this.StringSplitter.Split( targetMember.Name ).ToArray();
                if( splitNames.Length <= 1 ) isMatch = false;

                foreach( var splitName in splitNames )
                {
                    var members = sourceType.GetMember( splitName, bindingAttributes );
                    if( members == null || members.Length == 0 )
                    {
                        isMatch = false;
                        break;
                    }

                    sourceMember = members?[ 0 ];
                    if( sourceMember is MethodInfo )
                    {
                        var methodInfo = sourceMember as MethodInfo;
                        if( !methodInfo.IsGetterMethod() )
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    sourceType = sourceMember.GetMemberType();
                    sourceAccessPath.Add( sourceMember );
                }

                if( isMatch && this.MatchingRulesEvaluator.IsMatch( sourceAccessPath.Last(), targetMember ) )
                    yield return new MemberPair( sourceAccessPath, targetMember );
            }
        }

        //public IEnumerable<MemberPair> MapByConvention( Type source, Type target )
        //{
        //    if( MatchingRulesEvaluator == null )
        //        MatchingRulesEvaluator = new DefaultMatchingRuleEvaluator( this.MatchingRules );

        //    var memberPairs = this.GetMemberPairs( source, target, this.TargetMemberProvider )
        //        .Concat( this.GetMemberPairs( target, source, this.SourceMemberProvider ) );

        //    foreach( var memberPair in memberPairs )
        //        yield return memberPair;
        //}

        //private IEnumerable<MemberPair> GetMemberPairs( Type source, Type target, IMemberProvider sourceMemberProvider )
        //{
        //    var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
        //    if( !sourceMemberProvider.IgnoreNonPublicMembers )
        //        bindingAttributes |= BindingFlags.NonPublic;

        //    var sourceMembers = sourceMemberProvider.GetMembers( source ).ToList();
        //    foreach( var sourceMember in sourceMembers )
        //    {
        //        var targetAccessPath = new MemberAccessPath();

        //        Type targetType = target;
        //        MemberInfo targetMember = null;

        //        bool isMatch = true;
        //        var splitNames = this.StringSplitter.Split( sourceMember.Name ).ToArray();
        //        if( splitNames.Length <= 1 ) isMatch = false;

        //        foreach( var splitName in splitNames )
        //        {
        //            var members = targetType.GetMember( splitName, bindingAttributes );
        //            if( members == null || members.Length == 0 )
        //            {
        //                isMatch = false;
        //                break;
        //            }

        //            targetMember = members?[ 0 ];
        //            if( targetMember is MethodInfo )
        //            {
        //                var methodInfo = targetMember as MethodInfo;
        //                if( !methodInfo.IsGetterMethod() )
        //                {
        //                    isMatch = false;
        //                    break;
        //                }
        //            }

        //            targetType = targetMember.GetMemberType();
        //            targetAccessPath.Add( targetMember );
        //        }

        //        if( isMatch && this.MatchingRulesEvaluator.IsMatch( sourceMember, targetAccessPath.Last() ) )
        //            yield return new MemberPair( sourceMember, targetAccessPath );
        //    }
        //}
    }
}
