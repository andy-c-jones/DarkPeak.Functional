// Polyfill required for C# 15 union types in .NET 11 Preview 3.
// These BCL types are not yet shipped in the preview runtime and must be
// provided inline until they land in a later preview or GA release.
// See: https://oksala.net/2026/04/18/a-first-look-at-c-unions-in-net-11/
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    internal sealed class UnionAttribute : Attribute;

    internal interface IUnion
    {
        object? Value { get; }
    }
}
