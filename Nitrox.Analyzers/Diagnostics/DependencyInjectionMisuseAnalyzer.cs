using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     Dependency injection shouldn't be used in types that we can instantiate ourselves (i.e. not MonoBehaviours or Harmony patches).
///     We should use the Dependency Injection container only when we can apply the DI pattern to effect. If not, making said type static is often more readable and performant.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DependencyInjectionMisuseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.MisusedDependencyInjection);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(analysisContext =>
        {
            INamedTypeSymbol unityEngineObjectTypeSymbol = analysisContext.Compilation.GetTypeByMetadataName("UnityEngine.Object");
            INamedTypeSymbol nitroxPatchTypeSymbol = analysisContext.Compilation.GetTypeByMetadataName("NitroxPatcher.Patches.NitroxPatch");

            analysisContext.RegisterSyntaxNodeAction(c => AnalyzeDependencyInjectionMisuse(c, unityEngineObjectTypeSymbol, nitroxPatchTypeSymbol), SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeDependencyInjectionMisuse(SyntaxNodeAnalysisContext context, params INamedTypeSymbol[] allowedTypesUsingDependencyInjection)
    {
        InvocationExpressionSyntax expression = (InvocationExpressionSyntax)context.Node;
        if (expression.ChildNodes().FirstOrDefault(n => n is MemberAccessExpressionSyntax) is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return;
        }
        if (memberAccessExpression.DescendantNodes().FirstOrDefault(n => n is IdentifierNameSyntax) is not IdentifierNameSyntax memberAccessIdentifier)
        {
            return;
        }
        if (memberAccessIdentifier.Parent is TypeOfExpressionSyntax)
        {
            return;
        }
        if (!memberAccessIdentifier.GetName().Equals("NitroxServiceLocator", StringComparison.Ordinal))
        {
            return;
        }
        TypeDeclarationSyntax declaringType = expression.FindInParents<TypeDeclarationSyntax>();
        if (declaringType == null)
        {
            return;
        }
        INamedTypeSymbol declaringTypeSymbol = context.SemanticModel.GetDeclaredSymbol(declaringType);
        if (declaringTypeSymbol == null)
        {
            return;
        }
        foreach (INamedTypeSymbol allowedType in allowedTypesUsingDependencyInjection)
        {
            if (declaringTypeSymbol.IsType(allowedType))
            {
                return;
            }
        }

        Rules.ReportMisusedDependencyInjection(context, declaringType, memberAccessExpression.GetLocation());
    }

    private static class Rules
    {
        public static readonly DiagnosticDescriptor MisusedDependencyInjection = new("DIMA001",
                                                                                     "Dependency Injection container is used directly",
                                                                                     "The DI container should not be used directly in type '{0}' as the requested service can be supplied via a constructor parameter.",
                                                                                     "Usage",
                                                                                     DiagnosticSeverity.Warning,
                                                                                     true);

        public static void ReportMisusedDependencyInjection(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax declaringType, Location location)
        {
            context.ReportDiagnostic(Diagnostic.Create(MisusedDependencyInjection, location, declaringType.GetName()));
        }
    }
}
