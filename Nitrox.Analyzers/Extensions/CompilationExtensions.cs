using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Nitrox.Analyzers.Extensions;

internal static class CompilationExtensions
{
    /// <summary>
    ///     Returns the <see cref="INamedTypeSymbol" /> found or null.
    /// </summary>
    /// <param name="compilation">The compilation that should have the type.</param>
    /// <param name="assemblyNameWithoutDll">Assembly name, case sensitive.</param>
    /// <param name="fullTypeName">Type name include the namespace, case sensitive.</param>
    /// <returns></returns>
    public static INamedTypeSymbol GetType(this Compilation compilation, string assemblyNameWithoutDll, string fullTypeName) =>
        compilation.GetTypesByMetadataName(fullTypeName).FirstOrDefault(a => a.ContainingAssembly.Name.Equals(assemblyNameWithoutDll, StringComparison.Ordinal));
}
