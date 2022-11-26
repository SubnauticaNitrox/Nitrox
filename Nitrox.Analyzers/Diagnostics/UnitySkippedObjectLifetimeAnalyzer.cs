using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     Test that Unity objects are properly checked for their lifetime. The lifetime check is skipped when using "is null" as opposed to "== null".
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnitySkippedObjectLifetimeAnalyzer : DiagnosticAnalyzer
{
    private const string ANALYZER_ID = nameof(UnitySkippedObjectLifetimeAnalyzer);

    private static readonly DiagnosticDescriptor rule = new(ANALYZER_ID, "Tests that Unity object lifetime is not ignored", "'?.' on type {0} derives from 'UnityEngine.Object' which bypasses the Unity object lifetime check, use AliveOrNull instead",
                                                            "Usage", DiagnosticSeverity.Error, true, "Tests that Unity object lifetime checks are not ignored.");

    /// <summary>
    ///     Gets the list of rules of supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    /// <summary>
    ///     Initializes the analyzer by registering on symbol occurrence in the targeted code.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ConditionalAccessExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        static bool IsUnityObject(ITypeSymbol symbol)
        {
            string name = symbol.ToDisplayString();
            return name is "UnityEngine.GameObject" or "UnityEngine.Object";
        }
        
        static ITypeSymbol ExtractUnityObject(ITypeSymbol typeSymbol)
        {
            if (IsUnityObject(typeSymbol))
            {
                return typeSymbol;
            }
            ITypeSymbol baseType = typeSymbol;
            while (baseType?.BaseType != null)
            {
                baseType = baseType.BaseType;
                if (IsUnityObject(baseType))
                {
                    return baseType;
                }
            }
            return null;
        }
        
        ConditionalAccessExpressionSyntax expression = (ConditionalAccessExpressionSyntax)context.Node;
        ITypeSymbol originSymbol = context.SemanticModel.GetTypeInfo(expression.Expression).Type;
        ITypeSymbol unityObjectSymbol = ExtractUnityObject(originSymbol);
        if (unityObjectSymbol == null || (context.SemanticModel.GetSymbolInfo(expression.Expression).Symbol as IMethodSymbol)?.Name == "AliveOrNull")
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(rule, context.Node.GetLocation(), originSymbol!.Name));
    }
}
