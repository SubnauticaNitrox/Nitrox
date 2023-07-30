using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nitrox.Analyzers.Extensions;

public static class SyntaxExtensions
{
    public static bool IsPartialType(this TypeDeclarationSyntax typeSyntax) => typeSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);


    public static string GetNamespaceName(this TypeDeclarationSyntax type) => type.Ancestors()
                                                                                      .Select(n => n switch
                                                                                      {
                                                                                          FileScopedNamespaceDeclarationSyntax f => f.Name.ToString(),
                                                                                          NamespaceDeclarationSyntax ns => ns.Name.ToString(),
                                                                                          _ => null
                                                                                      })
                                                                                      .First();

    public static string GetReturnTypeName(this MemberDeclarationSyntax member) => member switch
    {
        FieldDeclarationSyntax field => field.Declaration.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ValueText ?? "",
        MethodDeclarationSyntax method => method.ReturnType.ToString(),
        _ => ""
    };

    public static string GetName(this MemberDeclarationSyntax member) => member switch
    {
        FieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? "",
        _ => ""
    };
}
