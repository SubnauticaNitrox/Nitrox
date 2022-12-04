using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     Test that Unity objects are properly checked for their lifetime.
///     The lifetime check is skipped when using "is null" or "obj?.member" as opposed to "== null".
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnitySkippedObjectLifetimeAnalyzer : DiagnosticAnalyzer
{
    public const string FIX_FUNCTION_NAME = "AliveOrNull";
    public const string FIX_FUNCTION_NAMESPACE = "NitroxClient.Unity.Helper";
    public const string CONDITIONAL_ACCESS_DIAGNOSTIC_ID = $"{nameof(UnitySkippedObjectLifetimeAnalyzer)}001";
    public const string IS_NULL_DIAGNOSTIC_ID = $"{nameof(UnitySkippedObjectLifetimeAnalyzer)}002";
    public const string NULL_COALESCE_DIAGNOSTIC_ID = $"{nameof(UnitySkippedObjectLifetimeAnalyzer)}003";
    private const string RULE_TITLE = "Tests that Unity object lifetime is not ignored";
    private const string RULE_DESCRIPTION = "Tests that Unity object lifetime checks are not ignored.";

    private static readonly DiagnosticDescriptor conditionalAccessRule = new(CONDITIONAL_ACCESS_DIAGNOSTIC_ID,
                                                                             RULE_TITLE,
                                                                             "'?.' is invalid on type '{0}' as it derives from 'UnityEngine.Object', bypassing the Unity object lifetime check",
                                                                             "Usage",
                                                                             DiagnosticSeverity.Error,
                                                                             true,
                                                                             RULE_DESCRIPTION);

    private static readonly DiagnosticDescriptor isNullRule = new(IS_NULL_DIAGNOSTIC_ID,
                                                                  RULE_TITLE,
                                                                  "'is null' is invalid on type '{0}' as it derives from 'UnityEngine.Object', bypassing the Unity object lifetime check",
                                                                  "Usage",
                                                                  DiagnosticSeverity.Error,
                                                                  true,
                                                                  RULE_DESCRIPTION);

    private static readonly DiagnosticDescriptor nullCoalesceRule = new(NULL_COALESCE_DIAGNOSTIC_ID,
                                                                        RULE_TITLE,
                                                                        "'??' is invalid on type '{0}' as it derives from 'UnityEngine.Object', bypassing the Unity object lifetime check",
                                                                        "Usage",
                                                                        DiagnosticSeverity.Error,
                                                                        true,
                                                                        RULE_DESCRIPTION);

    /// <summary>
    ///     Gets the list of rules of supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(conditionalAccessRule, isNullRule, nullCoalesceRule);

    /// <summary>
    ///     Initializes the analyzer by registering on symbol occurrence in the targeted code.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compStartContext =>
        {
            INamedTypeSymbol unityObjectTypeSymbol = compStartContext.Compilation.GetTypeByMetadataName("UnityEngine.Object");
            if (unityObjectTypeSymbol == null)
            {
                return;
            }

            compStartContext.RegisterSyntaxNodeAction(c => AnalyzeIsNullNode(c, unityObjectTypeSymbol), SyntaxKind.IsPatternExpression);
            compStartContext.RegisterSyntaxNodeAction(c => AnalyzeConditionalAccessNode(c, unityObjectTypeSymbol), SyntaxKind.ConditionalAccessExpression);
            compStartContext.RegisterSyntaxNodeAction(c => AnalyzeCoalesceNode(c, unityObjectTypeSymbol), SyntaxKind.CoalesceExpression);
        });
    }

    private void AnalyzeIsNullNode(SyntaxNodeAnalysisContext context, ITypeSymbol unityObjectSymbol)
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
        if (IsUnityObjectExpression(context, expression.Expression, unityObjectSymbol, out ITypeSymbol originSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(isNullRule, constantPattern.GetLocation(), originSymbol!.Name));
        }
    }

    private void AnalyzeConditionalAccessNode(SyntaxNodeAnalysisContext context, ITypeSymbol unityObjectSymbol)
    {
        static bool IsFixedWithAliveOrNull(SyntaxNodeAnalysisContext context, ConditionalAccessExpressionSyntax expression)
        {
            return (context.SemanticModel.GetSymbolInfo(expression.Expression).Symbol as IMethodSymbol)?.Name == FIX_FUNCTION_NAME;
        }

        ConditionalAccessExpressionSyntax expression = (ConditionalAccessExpressionSyntax)context.Node;
        if (IsUnityObjectExpression(context, expression.Expression, unityObjectSymbol, out ITypeSymbol originSymbol) && !IsFixedWithAliveOrNull(context, expression))
        {
            context.ReportDiagnostic(Diagnostic.Create(conditionalAccessRule, context.Node.GetLocation(), originSymbol!.Name));
        }
    }

    private void AnalyzeCoalesceNode(SyntaxNodeAnalysisContext context, ITypeSymbol unityObjectSymbol)
    {
        BinaryExpressionSyntax expression = (BinaryExpressionSyntax)context.Node;
        if (IsUnityObjectExpression(context, expression.Left, unityObjectSymbol, out ITypeSymbol originSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(nullCoalesceRule, context.Node.GetLocation(), originSymbol!.Name));
        }
    }

    private bool IsUnityObjectExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax possibleUnityAccessExpression, ITypeSymbol compareSymbol, out ITypeSymbol possibleUnitySymbol)
    {
        possibleUnitySymbol = context.SemanticModel.GetTypeInfo(possibleUnityAccessExpression).Type;
        return possibleUnitySymbol.IsType(compareSymbol);
    }
}
