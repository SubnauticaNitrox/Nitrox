using System.Collections;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     Test that calls to a method returning an IEnumerator are iterated (MoveNext is called). If they aren't iterated than the code in them won't
///     continue after the first 'yield return'.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumeratorUsageAnalyzer : DiagnosticAnalyzer
{
    public const string UNUSED_ENUMERATOR = $"{nameof(EnumeratorUsageAnalyzer)}001";

    private static readonly DiagnosticDescriptor unusedEnumerator = new(UNUSED_ENUMERATOR,
                                                                        "IEnumerator is not iterated",
                                                                        $"The IEnumerator '{{0}}' must be iterated by calling its {nameof(IEnumerator.MoveNext)} otherwise it will stop executing at the first 'yield return' expression",
                                                                        "Usage",
                                                                        DiagnosticSeverity.Warning,
                                                                        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(unusedEnumerator);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(static c => AnalyzeIEnumeratorInvocation(c), SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeIEnumeratorInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax expression = (InvocationExpressionSyntax)context.Node;
        if (expression.Parent == null)
        {
            return;
        }
        IMethodSymbol methodSymbol = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }
        // Ignore if method invoke is used/wrapped by something (variable declaration, as a parameter, etc).
        if (!expression.Parent.IsKind(SyntaxKind.ExpressionStatement))
        {
            return;
        }
        if (!methodSymbol.ReturnType.IsType(context.SemanticModel, "System.Collections.IEnumerator"))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(unusedEnumerator, expression.GetLocation(), methodSymbol.Name));
    }
}
