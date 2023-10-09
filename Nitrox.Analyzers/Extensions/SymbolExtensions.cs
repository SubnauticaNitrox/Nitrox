using Microsoft.CodeAnalysis;

namespace Nitrox.Analyzers.Extensions;

public static class SymbolExtensions
{
    public static bool IsType(this ITypeSymbol symbol, SemanticModel semanticModel, string fullyQualifiedTypeName)
    {
        return symbol.IsType(semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedTypeName));
    }

    public static bool IsType(this ITypeSymbol symbol, ITypeSymbol targetingSymbol)
    {
        if (symbol == null || targetingSymbol == null)
        {
            return false;
        }
        if (SymbolEqualityComparer.Default.Equals(symbol, targetingSymbol))
        {
            return true;
        }
        while (symbol.BaseType is { } baseTypeSymbol)
        {
            symbol = baseTypeSymbol;
            if (SymbolEqualityComparer.Default.Equals(symbol, targetingSymbol))
            {
                return true;
            }
        }
        return false;
    }
}
