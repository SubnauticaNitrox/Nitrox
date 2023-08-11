using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     NitroxServiceLocator shouldn't be used in types not inheriting from NitroxPatch or MonoBehaviour
///     as we should use the Dependency Injection container only when we can apply the DI pattern to effect.
///     It's often more readable to use static in the other cases like done for GameInstallationFinder
///     as it's easy to see that code is used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AutoFacUsageAnalyzer : DiagnosticAnalyzer
{
    public const string MISUSED_AUTOFAC = $"{nameof(AutoFacUsageAnalyzer)}001";

    private static readonly DiagnosticDescriptor misusedAutofac = new(MISUSED_AUTOFAC,
                                                                        "Dependency Injection container is used directly",
                                                                        $"The DI container should not be used directly in type '{{0}}' as the requested service can be supplied via a constructor parameter.",
                                                                        "Usage",
                                                                        DiagnosticSeverity.Warning,
                                                                        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(misusedAutofac);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(static c => AnalyzeServiceLocatorMisuse(c), SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(static c => AnalyzeResolveMisuse(c), SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeServiceLocatorMisuse(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax expression = (InvocationExpressionSyntax)context.Node;
        // To avoid calls of NitroxServiceLocator in nameof() or type(), we seek for this precise structure
        if (expression.ChildNodes().FirstOrDefault(n => n is MemberAccessExpressionSyntax) is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return;
        }
        if (memberAccessExpression.ChildNodes().FirstOrDefault(n => n is IdentifierNameSyntax) is not IdentifierNameSyntax memberAccessIdentifier)
        {
            return;
        }
        if (memberAccessIdentifier.GetName() != "NitroxServiceLocator")
        {
            return;
        }
        AnalyzeContext(context, expression);
    }
    private static void AnalyzeResolveMisuse(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax expression = (InvocationExpressionSyntax)context.Node;
        // To avoid calls of Resolve in nameof() or type(), we seek for this precise structure
        if (expression.ChildNodes().FirstOrDefault(n => n is GenericNameSyntax) is not GenericNameSyntax genericNameSyntax)
        {
            return;
        }
        if (!genericNameSyntax.Identifier.Text.Equals("Resolve"))
        {
            return;
        }
        AnalyzeContext(context, expression);
    }

    private static void AnalyzeContext(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
    {
        TypeDeclarationSyntax declaredType = expression.FindInParents<TypeDeclarationSyntax>();
        if (declaredType == null)
        {
            return;
        }
        INamedTypeSymbol declaredTypeSymbol = context.SemanticModel.GetDeclaredSymbol(declaredType);
        if (declaredTypeSymbol == null)
        {
            return;
        }
        // Valid places to use DI
        if (declaredTypeSymbol.IsType(context.SemanticModel, "UnityEngine.Object") ||
            declaredTypeSymbol.IsType(context.SemanticModel, "NitroxPatcher.Patches.NitroxPatch"))
        {
            return;
        }
        // Nitrox.Test uses unconventionally DI but it's normal
        if (declaredTypeSymbol.ContainingAssembly.Name.Equals("Nitrox.Test"))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(misusedAutofac, expression.GetLocation(), declaredType.GetName()));
    }
}
