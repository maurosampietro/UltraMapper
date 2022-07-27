/*
    Enables C#9 record and init property with target framework < NET50
*/

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    internal static class IsExternalInit
    {
    }
}
#endif