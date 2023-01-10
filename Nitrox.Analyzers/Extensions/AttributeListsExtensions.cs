using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nitrox.Analyzers.Extensions;

internal static class AttributeListsExtensions
{
    public static bool HasAttribute(this SyntaxList<AttributeListSyntax> list, string name)
    {
        foreach (AttributeListSyntax innerList in list)
        {
            foreach (AttributeSyntax attribute in innerList.Attributes)
            {
                if (attribute.Name.GetValue().Equals(name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
