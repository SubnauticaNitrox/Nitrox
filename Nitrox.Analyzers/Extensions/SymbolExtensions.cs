using Microsoft.CodeAnalysis;

namespace Nitrox.Analyzers.Extensions;

public static class SymbolExtensions
{
    /// <summary>
    ///     Returns true if symbol points to a type that is (or inherits from) the given namespace and type.
    /// </summary>
    public static bool IsType(this ITypeSymbol symbol, string typeName, string fullNamespace)
    {
        if (symbol.Name == typeName && symbol.ContainingNamespace.Name == fullNamespace)
        {
            return true;
        }
        while (symbol.BaseType is { } baseTypeSymbol)
        {
            symbol = baseTypeSymbol;
            if (symbol.Name == typeName && symbol.ContainingNamespace.Name == fullNamespace)
            {
                return true;
            }
        }
        return false;
    }
}
