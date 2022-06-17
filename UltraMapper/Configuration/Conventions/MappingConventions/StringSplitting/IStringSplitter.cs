using System.Collections.Generic;

namespace UltraMapper.Conventions
{
    public interface IStringSplitter
    {
        IEnumerable<string> Split( string str );
    }
}