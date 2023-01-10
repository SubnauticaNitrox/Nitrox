using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Generators;

/// <summary>
///     Generates a serializable attribute that can be used to make (and mark) a type serializable.
///     Only classes are supported because the attribute needed for the partial class metadata attributes only works on classes, not structs.
/// </summary>
/// <remarks>
///     <para>
///         <b>Potential solutions</b><br />
///         - Rewrite (private) structs as (public) classes via source gen.<br />
///         - Generate the models for the serializers instead of indirectly via attributes.<br />
///     </para>
///     <a href="https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md">Source generator docs</a>
/// </remarks>
[Generator(LanguageNames.CSharp)]
internal sealed class NitroxSerializableGenerator : IIncrementalGenerator
{
    private const string NITROX_SERIALIZABLE_ATTRIBUTE_NAME = "NitroxSerializable";

    private static readonly ImmutableArray<string> requiredUsingStatements = ImmutableArray.Create("System", "System.Runtime.Serialization", "System.ComponentModel.DataAnnotations", "BinaryPack.Attributes");

    [SuppressMessage("ReSharper", "SuggestVarOrType_Elsewhere")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute. To be used for marking types as serializable.
        context.RegisterPostInitializationOutput(GenerateSerializableAttribute);

        // Setup compilation pipeline that also verifies that serializer dependencies exist for the targeted assembly.
        var compilationPipeline = context.CompilationProvider.Select((c, _) =>
        {
            TypeSymbolRequirement metadataTypeSymbolAttr = new(null, "System.ComponentModel.DataAnnotations", "System.ComponentModel.DataAnnotations.MetadataTypeAttribute");
            INamedTypeSymbol metaDataTypeAttr = c.HasType(metadataTypeSymbolAttr.AssemblyName, metadataTypeSymbolAttr.FullTypeName);
            metadataTypeSymbolAttr = metadataTypeSymbolAttr with { Symbol = metaDataTypeAttr };

            TypeSymbolRequirement ignoreConstructorSymbolAttr = new(null, "Nitrox.BinaryPack", "BinaryPack.Attributes.IgnoreConstructorAttribute");
            INamedTypeSymbol ignoreConstructorAttr = c.HasType(metadataTypeSymbolAttr.AssemblyName, metadataTypeSymbolAttr.FullTypeName);
            ignoreConstructorSymbolAttr = ignoreConstructorSymbolAttr with { Symbol = ignoreConstructorAttr };

            return ImmutableArray.Create(metadataTypeSymbolAttr, ignoreConstructorSymbolAttr);
        });
        // Look for types with our serializable attribute.
        var typesPipeline = context.SyntaxProvider
                                   .CreateSyntaxProvider(
                                       static (node, _) => IsSyntaxTargetForGeneration(node),
                                       static (context, _) => GetSemanticTargetForGeneration(context))
                                   .Where(static result => result != null);

        // Register the pipeline into the compiler.
        var combinedPipeline = typesPipeline.Collect().Combine(compilationPipeline);
        context.RegisterSourceOutput(combinedPipeline, static (context, source) => Execute(source.Right, source.Left, context));
    }

    private static void Execute(ImmutableArray<TypeSymbolRequirement> typeSymbolRequirements, ImmutableArray<TypeDeclarationSyntax> types, SourceProductionContext context)
    {
        if (types.IsDefaultOrEmpty)
        {
            return;
        }
        bool requirementsMet = true;
        foreach (TypeSymbolRequirement requirement in typeSymbolRequirements.Where(req => !req.Exists))
        {
            requirementsMet = false;
            Rules.ReportMissingDependency(context, types.FirstOrDefault(), requirement);
        }
        if (!requirementsMet)
        {
            return;
        }

        foreach (TypeDeclarationSyntax typeDeclare in types)
        {
            if (!typeDeclare.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                Rules.ReportMissingPartialKeyword(context, typeDeclare);
                continue;
            }
            if (typeDeclare.SyntaxTree.GetRoot() is not CompilationUnitSyntax compRoot)
            {
                continue;
            }

            // Prepare syntax for new code file: Reuse entry class, defining an inner class that has the serializable fields with serializable attributes.
            string entryType = typeDeclare.Identifier.ValueText;
            string entryNamespace = GetNamespaceFromType(typeDeclare);
            string entryMetadataClassName = $"_{entryType}Metadata";
            SyntaxList<AttributeListSyntax> metaDataAttribute = CreateSingletonAttributeLists(("MetadataType", new[] { SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(entryMetadataClassName))) }));
            SyntaxList<AttributeListSyntax> serializableTypeAttributes = CreateSingletonAttributeLists(("Serializable", null), ("DataContract", null));
            MemberDeclarationSyntax metaDataClass = SyntaxFactory.ClassDeclaration(entryMetadataClassName)
                                                                 .WithAttributeLists(serializableTypeAttributes)
                                                                 .WithModifiers(new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                                                                 .WithMembers(new SyntaxList<MemberDeclarationSyntax>(CreateSerializableMembersFromClass(typeDeclare)));

            // Emit new code.
            context.AddSource($"{entryNamespace}.{entryType}.g.cs", $$"""
            {{compRoot.Usings.EnsureUsing(requiredUsingStatements)}}

            namespace {{entryNamespace}};

            {{typeDeclare
              .WithAttributeLists(metaDataAttribute)
              .WithBaseList(null)
              .WithMembers(new SyntaxList<MemberDeclarationSyntax>(CreateSerializableConstructorMember(typeDeclare)).Add(metaDataClass))
              .NormalizeWhitespace()}}
            """);
        }
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node switch
    {
        ClassDeclarationSyntax c => c.AttributeLists.HasAttribute(NITROX_SERIALIZABLE_ATTRIBUTE_NAME),
        _ => false
    };

    private static TypeDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context) => (TypeDeclarationSyntax)context.Node;

    private static string GetNamespaceFromType(TypeDeclarationSyntax type) => type.Ancestors()
                                                                                  .Select(n => n switch
                                                                                  {
                                                                                      FileScopedNamespaceDeclarationSyntax f => f.Name.ToString(),
                                                                                      NamespaceDeclarationSyntax ns => ns.Name.ToString(),
                                                                                      _ => null
                                                                                  })
                                                                                  .First();

    private static IEnumerable<MemberDeclarationSyntax> CreateSerializableConstructorMember(TypeDeclarationSyntax typeDeclare)
    {
        ImmutableArray<ConstructorDeclarationSyntax> ctors = typeDeclare.Members.Where(m => m.IsKind(SyntaxKind.ConstructorDeclaration))
                                                                        .Select(m => m as ConstructorDeclarationSyntax)
                                                                        .Where(ctor => ctor != null)
                                                                        .ToImmutableArray();
        // Add a default parameterless constructor used by JSON serialization (if it does not exist).
        SyntaxKind ctorKind;
        if (typeDeclare.Modifiers.Any(SyntaxKind.SealedKeyword))
        {
            ctorKind = SyntaxKind.PrivateKeyword;
        }
        else
        {
            ctorKind = SyntaxKind.ProtectedKeyword;
        }
        if (ctors.Length == 0)
        {
            ctorKind = SyntaxKind.PublicKeyword;
        }
        if (ctors.All(ctor => ctor.ParameterList.Parameters.Count != 0))
        {
            SyntaxList<AttributeListSyntax> attributes = default;
            // If other constructors exist, mark the new parameterless constructor as "IgnoreConstructor" for BinaryPack.
            if (ctors.Any(c => c.ParameterList.Parameters.Count > 0))
            {
                attributes = CreateSingletonAttributeLists(("IgnoreConstructor", null));
            }
            yield return SyntaxFactory.ConstructorDeclaration(attributes, SyntaxTokenList.Create(SyntaxFactory.Token(ctorKind)), typeDeclare.Identifier, SyntaxFactory.ParameterList(), default(ConstructorInitializerSyntax),
                                                              SyntaxFactory.Block());
        }
    }

    private static IEnumerable<MemberDeclarationSyntax> CreateSerializableMembersFromClass(TypeDeclarationSyntax typeDeclare)
    {
        // Return the serializable members in the type as-is but with the serializable attributes.
        IEnumerable<MemberDeclarationSyntax> fieldsAndProperties = typeDeclare.Members
                                                                              .Where(m => !m.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                                                                                          !m.IsKind(SyntaxKind.MethodDeclaration) &&
                                                                                          !m.IsKind(SyntaxKind.ConstructorDeclaration) &&
                                                                                          !m.IsKind(SyntaxKind.ClassDeclaration) &&
                                                                                          !m.IsKind(SyntaxKind.StructDeclaration))
                                                                              .Where(m => !m.AttributeLists.HasAttribute("IgnoreDataMember"))
                                                                              .Where(m => m is not PropertyDeclarationSyntax property ||
                                                                                          (property.ExpressionBody == null &&
                                                                                           (!property.AccessorList?.ChildNodes().All(node => node is AccessorDeclarationSyntax { Body: { } }) ?? true)))
                                                                              .Select(m => m switch
                                                                              {
                                                                                  // Remove code that isn't needed in the generated metadata type for assigning attributes to members.
                                                                                  PropertyDeclarationSyntax { Initializer: { } } property => property.WithInitializer(default(EqualsValueClauseSyntax)).WithSemicolonToken(default(SyntaxToken)),
                                                                                  _ => m
                                                                              })
                                                                              .Select((m, i) => m.WithAttributeLists(
                                                                                          CreateSingletonAttributeLists(
                                                                                              ("DataMember",
                                                                                                  new[]
                                                                                                  {
                                                                                                      SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i + 1)))
                                                                                                                   .WithNameEquals(SyntaxFactory.NameEquals("Order"))
                                                                                                  }))));
        foreach (MemberDeclarationSyntax fieldOrProperty in fieldsAndProperties)
        {
            yield return fieldOrProperty;
        }
    }

    private static SyntaxList<AttributeListSyntax> CreateSingletonAttributeLists(params (string name, AttributeArgumentSyntax[] args)[] attributes) => new(
        SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributes.Select(attribute =>
        {
            AttributeArgumentListSyntax attributeArguments = attribute.args != null ? SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(attribute.args)) : null;
            AttributeSyntax result = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attribute.name));
            if (attributeArguments != null)
            {
                result = result.WithArgumentList(attributeArguments);
            }
            return result;
        }))));

    private static void GenerateSerializableAttribute(IncrementalGeneratorPostInitializationContext context) => context.AddSource($"{NITROX_SERIALIZABLE_ATTRIBUTE_NAME}Attribute.g.cs", $$"""
        using System;

        /// <summary>
        ///     Attribute that marks a type as serializable. If marked type is partial, generates the required constructor and attributes on fields for serialization.
        ///     Apply the IgnoreDataMember attribute to exclude a field from serialization.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        internal class {{NITROX_SERIALIZABLE_ATTRIBUTE_NAME}}Attribute : Attribute
        {
        }
        """);

    private readonly record struct TypeSymbolRequirement(INamedTypeSymbol Symbol, string AssemblyName, string FullTypeName)
    {
        public bool Exists => Symbol != null;
    }

    private static class Rules
    {
        public const string MISSING_DEPENDENCY_DIAGNOSTIC_ID = $"{nameof(NitroxSerializableGenerator)}001";
        public const string MISSING_KEYWORD_DIAGNOSTIC_ID = $"{nameof(NitroxSerializableGenerator)}002";

        internal static readonly DiagnosticDescriptor MissingDependencyRule = new(MISSING_DEPENDENCY_DIAGNOSTIC_ID,
                                                                                  "Tests that the necessary dependency is available for generating the serializable attributes.",
                                                                                  $"Current project is missing dependency '{{0}}' to use type '{{1}}' for serialization with {NITROX_SERIALIZABLE_ATTRIBUTE_NAME}",
                                                                                  "Usage",
                                                                                  DiagnosticSeverity.Error,
                                                                                  true);

        internal static readonly DiagnosticDescriptor MissingPartialKeywordRule = new(MISSING_KEYWORD_DIAGNOSTIC_ID,
                                                                                       "Tests that the class is marked as partial.",
                                                                                       $"Class '{{0}}' must be marked as partial to use {NITROX_SERIALIZABLE_ATTRIBUTE_NAME}",
                                                                                       "Usage",
                                                                                       DiagnosticSeverity.Error,
                                                                                       true);

        internal static void ReportMissingDependency(SourceProductionContext context, TypeDeclarationSyntax type, TypeSymbolRequirement requirement)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingDependencyRule, type.Identifier.GetLocation(), requirement.AssemblyName, requirement.FullTypeName));
        }

        internal static void ReportMissingPartialKeyword(SourceProductionContext context, TypeDeclarationSyntax type)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingPartialKeywordRule, type.Identifier.GetLocation(), type.Identifier.ValueText));
        }
    }
}
