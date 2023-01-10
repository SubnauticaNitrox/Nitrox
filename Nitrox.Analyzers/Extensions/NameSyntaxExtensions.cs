using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nitrox.Analyzers.Extensions;

internal static class NameSyntaxExtensions
{
    public static string GetValue(this NameSyntax syntax) => syntax switch
    {
        SimpleNameSyntax simple => simple.Identifier.ValueText,
        { } => syntax.ToString(),
        _ => ""
    };
}
