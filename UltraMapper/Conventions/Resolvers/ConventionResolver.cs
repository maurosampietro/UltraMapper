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

    public class FlatteningConventionResolver : IConventionResolver
    {
        public IMemberProvider SourceMemberProvider { get; set; }
        public IMemberProvider TargetMemberProvider { get; set; }

        public StringSplitter Splitter { get; set; }

        public FlatteningConventionResolver()
            : this( new SourceMemberProvider(), new TargetMemberProvider() ) { }

        public FlatteningConventionResolver( IMemberProvider sourceMemberProvider,
            IMemberProvider targetMembetProvider )
        {
            this.SourceMemberProvider = sourceMemberProvider;
            this.TargetMemberProvider = targetMembetProvider;

            this.Splitter = new StringSplitter( new PascalCaseSplittingRule() );
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
                var splitNames = this.Splitter.Split( targetMember.Name );

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

    public sealed class StringSplitter
    {
        public ISplittingRule SplittingRule { get; set; }

        public StringSplitter( ISplittingRule splittingRule )
        {
            this.SplittingRule = splittingRule;
        }

        public IEnumerable<string> Split( string str )
        {
            if( String.IsNullOrEmpty( str ) ) yield break;

            var substring = new StringBuilder( str.Length );
            substring.Append( str[ 0 ] );

            for( int i = 1; i < str.Length; i++ )
            {
                if( this.SplittingRule.IsSplitChar( str[ i ] ) )
                {
                    yield return substring.ToString();

                    substring.Clear();
                    substring.Append( str[ i ] );
                }
                else
                {
                    substring.Append( str[ i ] );
                }
            }

            if( substring.Length > 0 )
                yield return substring.ToString();
        }
    }

    public interface ISplittingRule
    {
        bool IsSplitChar( char c );
    }

    /// <summary>
    /// Informs the caller to split if an upper case character is encountered
    /// </summary>
    public class PascalCaseSplittingRule : ISplittingRule
    {
        public bool IsSplitChar( char c ) => Char.IsUpper( c );
    }

    /// <summary>
    /// Informs the caller to split if an underscore character is encountered
    /// </summary>
    public class UnderscoreSplittingRule : ISplittingRule
    {
        public bool IsSplitChar( char c ) => c == '_';
    }
}
