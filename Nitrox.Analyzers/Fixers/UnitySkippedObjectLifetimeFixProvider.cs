extern alias JB;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JB::JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Nitrox.Analyzers.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Nitrox.Analyzers.Fixers;

[Shared]
[UsedImplicitly]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnitySkippedObjectLifetimeFixProvider))]
public sealed class UnitySkippedObjectLifetimeFixProvider : CodeFixProvider
{
    private const string ALIVE_OR_NULL_REQUIRED_USING_NAME = "NitroxClient.Unity.Helper";
    private static readonly IdentifierNameSyntax aliveOrNull = IdentifierName("AliveOrNull");
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UnitySkippedObjectLifetimeAnalyzer.CONDITIONAL_ACCESS_DIAGNOSTIC_ID);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Code template from: https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix
        SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
        ConditionalAccessExpressionSyntax declaration = root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                                                             .OfType<ConditionalAccessExpressionSyntax>()
                                                             .First();
        context.RegisterCodeFix(
            CodeAction.Create(
                equivalenceKey: UnitySkippedObjectLifetimeAnalyzer.CONDITIONAL_ACCESS_DIAGNOSTIC_ID,
                title: "Insert AliveOrNull() before conditional access of UnityEngine.Object",
                createChangedDocument: c => InsertAliveOrNullAsync(context.Document, declaration, c)
            ),
            diagnostic);
    }

    private async Task<Document> InsertAliveOrNullAsync(Document document, ConditionalAccessExpressionSyntax declaration, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // 1. Wrap expression with an invocation to AliveOrNull, this will cause AliveOrNull to be called before the conditional access.
        InvocationExpressionSyntax wrappedExpression = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, declaration.Expression, aliveOrNull));
        SyntaxNode newDeclaration = declaration.ReplaceNode(declaration.Expression, wrappedExpression);
        root = root!.ReplaceNode(declaration, newDeclaration);
        // 2. Ensure using statement for extension method .AliveOrNull().
        // This is done after the "AliveOrNull" wrap because the declaration instance can't be found when root instance updates.
        if (root is CompilationUnitSyntax compilationRoot && compilationRoot.Usings.All(u => u.Name.ToString() != ALIVE_OR_NULL_REQUIRED_USING_NAME))
        {
            root = compilationRoot.AddUsings(UsingDirective(IdentifierName(ALIVE_OR_NULL_REQUIRED_USING_NAME)));
        }

        // Replace the old document with the new.
        return document.WithSyntaxRoot(root);
    }
}
