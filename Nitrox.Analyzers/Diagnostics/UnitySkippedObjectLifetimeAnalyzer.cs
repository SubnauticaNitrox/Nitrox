using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     Test that Unity objects are properly checked for their lifetime.
///     The lifetime check is skipped when using "is null" or "obj?.member" as opposed to "== null".
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnitySkippedObjectLifetimeAnalyzer : DiagnosticAnalyzer
{
    public const string CONDITIONAL_ACCESS_DIAGNOSTIC_ID = nameof(UnitySkippedObjectLifetimeAnalyzer) + "001";
    public const string IS_NULL_DIAGNOSTIC_ID = nameof(UnitySkippedObjectLifetimeAnalyzer) + "002";

    private static readonly DiagnosticDescriptor conditionalAccessRule = new(CONDITIONAL_ACCESS_DIAGNOSTIC_ID,
                                                                             "Tests that Unity object lifetime is not ignored",
                                                                             "'?.' is invalid on type '{0}' as it derives from 'UnityEngine.Object', bypassing the Unity object lifetime check",
                                                                             "Usage",
                                                                             DiagnosticSeverity.Error,
                                                                             true,
                                                                             "Tests that Unity object lifetime checks are not ignored.");

    private static readonly DiagnosticDescriptor isNullRule = new(IS_NULL_DIAGNOSTIC_ID,
                                                                  "Tests that Unity object lifetime is not ignored",
                                                                  "'is null' is invalid on type '{0}' as it derives from 'UnityEngine.Object', bypassing the Unity object lifetime check",
                                                                  "Usage",
                                                                  DiagnosticSeverity.Error,
                                                                  true,
                                                                  "Tests that Unity object lifetime checks are not ignored.");

    /// <summary>
    ///     Gets the list of rules of supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(conditionalAccessRule, isNullRule);

    /// <summary>
    ///     Initializes the analyzer by registering on symbol occurrence in the targeted code.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIsNullNode, SyntaxKind.IsPatternExpression);
        context.RegisterSyntaxNodeAction(AnalyzeConditionalAccessNode, SyntaxKind.ConditionalAccessExpression);
    }

    private void AnalyzeIsNullNode(SyntaxNodeAnalysisContext context)
    {
        IsPatternExpressionSyntax expression = (IsPatternExpressionSyntax)context.Node;
        // Is this a "is null" check?
        if (expression.Pattern is not ConstantPatternSyntax constantPattern)
        {
            return;
        }
        if (constantPattern.Expression is not LiteralExpressionSyntax literal || !literal.Token.IsKind(SyntaxKind.NullKeyword))
        {
            return;
        }
        // Is it on a UnityEngine.Object?
        ITypeSymbol originSymbol = context.SemanticModel.GetTypeInfo(expression.Expression).Type;
        ITypeSymbol unityObjectSymbol = ExtractUnityObject(originSymbol);
        if (unityObjectSymbol == null)
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(isNullRule, constantPattern.GetLocation(), originSymbol!.Name));
    }

    private void AnalyzeConditionalAccessNode(SyntaxNodeAnalysisContext context)
    {
        static bool IsFixedWithAliveOrNull(SyntaxNodeAnalysisContext context, ConditionalAccessExpressionSyntax expression)
        {
            return (context.SemanticModel.GetSymbolInfo(expression.Expression).Symbol as IMethodSymbol)?.Name == "AliveOrNull";
        }

        ConditionalAccessExpressionSyntax expression = (ConditionalAccessExpressionSyntax)context.Node;
        ITypeSymbol originSymbol = context.SemanticModel.GetTypeInfo(expression.Expression).Type;
        ITypeSymbol unityObjectSymbol = ExtractUnityObject(originSymbol);
        if (unityObjectSymbol == null || IsFixedWithAliveOrNull(context, expression))
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(conditionalAccessRule, context.Node.GetLocation(), originSymbol!.Name));
    }

    private bool IsUnityObject(ITypeSymbol symbol)
    {
        string name = symbol.ToDisplayString();
        return name is "UnityEngine.GameObject" or "UnityEngine.Object";
    }

    private ITypeSymbol ExtractUnityObject(ITypeSymbol typeSymbol)
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
}
