using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nitrox.Analyzers.Extensions;

internal static class SyntaxListExtensions
{
    public static SyntaxList<UsingDirectiveSyntax> EnsureUsing(this SyntaxList<UsingDirectiveSyntax> list, ImmutableArray<string> usingNamespaces)
    {
        return list.AddRange(usingNamespaces.Except(list.Select(i => i.Name.GetValue())).Select(usingNamespace => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(usingNamespace)).NormalizeWhitespace()));
    }
}
