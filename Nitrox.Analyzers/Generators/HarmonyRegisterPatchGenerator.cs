using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Generators;

/// <summary>
///     Implements the harmony patch registry boilerplate for NitroxPatch inherited types by scanning its static MethodInfo
///     fields and static patch methods.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed class HarmonyRegisterPatchGenerator : IIncrementalGenerator
{
    private static readonly string[] harmonyMethodTypes = { "prefix", "postfix", "transpiler", "finalizer", "manipulator" };
    private static readonly string[] validTargetMethodNames = { "target_method", "targetmethod", "target", "method" };
    private static readonly Lazy<string> generatedCodeAttribute = new(() => $@"[global::System.CodeDom.Compiler.GeneratedCode(""{typeof(HarmonyRegisterPatchGenerator).FullName}"", ""{typeof(HarmonyRegisterPatchGenerator).Assembly.GetName().Version}"")]");

    [SuppressMessage("ReSharper", "SuggestVarOrType_Elsewhere")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Setup compilation pipeline for assemblies that use NitroxPatch.
        var compilationPipeline = context.CompilationProvider.Select((c, _) => c.GetType("NitroxPatcher", "NitroxPatcher.Patches.NitroxPatch") != null);
        // Look for partial types inheriting our NitroxPatch type, selecting all the harmony methods and target method infos.
        var harmonyMethodsWithTargetMethods = context.SyntaxProvider
                                                     .CreateSyntaxProvider(
                                                         static (node, _) => IsSyntaxTargetForGeneration(node),
                                                         static (context, _) => GetSemanticTargetForGeneration(context))
                                                     .Where(r => r is not null)
                                                     .WithComparer(NitroxHarmonyType.NitroxHarmonyTypeEqualityComparer.Instance);

        // Register the pipeline into the compiler.
        var combinedPipeline = harmonyMethodsWithTargetMethods.Combine(compilationPipeline);
        context.RegisterSourceOutput(combinedPipeline, static (context, source) => Execute(context, source.Left));
    }

    private static void Execute(SourceProductionContext context, NitroxHarmonyType nitroxHarmonyType)
    {
        // Build Patch method implementation.
        StringBuilder patchImpl = new();
        for (int fieldIndex = 0; fieldIndex < nitroxHarmonyType.MethodInfoFields.Length; fieldIndex++)
        {
            FieldDeclarationSyntax methodInfoField = nitroxHarmonyType.MethodInfoFields[fieldIndex];
            patchImpl.Append("PatchMultiple(harmony, ")
                     .Append(methodInfoField.GetName());
            if (nitroxHarmonyType.HarmonyPatchMethods.Length > 0)
            {
                patchImpl.Append(", ");
                foreach (MethodDeclarationSyntax harmonyPatchMethod in nitroxHarmonyType.HarmonyPatchMethods)
                {
                    patchImpl.Append(harmonyPatchMethod.Identifier.ValueText.ToLowerInvariant())
                             .Append(": ((Delegate)")
                             .Append(harmonyPatchMethod.Identifier.ValueText)
                             .Append(").Method, ");
                }
                patchImpl.Remove(patchImpl.Length - 2, 2);
            }
            patchImpl.Append(");");
            // Append new line if not last implementation line.
            if (fieldIndex < nitroxHarmonyType.MethodInfoFields.Length - 1)
            {
                patchImpl.AppendLine();
            }
        }

        // Append new code to the compilation.
        context.AddSource($"{nitroxHarmonyType.NameSpace}.{nitroxHarmonyType.TypeName}.g.cs", $$"""
        #pragma warning disable
        using System;
        using HarmonyLib;

        namespace {{nitroxHarmonyType.NameSpace}};

        partial class {{nitroxHarmonyType.TypeName}}
        {
            {{generatedCodeAttribute.Value}}
            public override void Patch(Harmony harmony)
            {
                {{patchImpl}}
            }
        }
        """);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is not TypeDeclarationSyntax type)
        {
            return false;
        }
        if (!type.IsPartialType())
        {
            return false;
        }
        // Skip if not deriving from "NitroxPatch".
        if (type.BaseList?.Types.FirstOrDefault(t => t.ToString().Equals("NitroxPatch", StringComparison.Ordinal)) == null)
        {
            return false;
        }
        // Skip if "Patch" method is already defined.
        if (type.Members.OfType<MethodDeclarationSyntax>().Any(m => m.Modifiers.Any(SyntaxKind.OverrideKeyword) && m.Identifier.ValueText == "Patch"))
        {
            return false;
        }
        return true;
    }

    private static NitroxHarmonyType GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        static bool IsValidPatchMethodName(string methodName) => harmonyMethodTypes.Contains(methodName.ToLowerInvariant());

        static bool IsValidTargetMethodFieldName(string fieldName)
        {
            foreach (string n in validTargetMethodNames)
            {
                if (fieldName.StartsWith(n, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        TypeDeclarationSyntax type = context.Node as TypeDeclarationSyntax;
        if (type == null)
        {
            return null;
        }
        ImmutableArray<MemberDeclarationSyntax> members = type.Members.ToImmutableArray();
        return new NitroxHarmonyType(type.GetNamespaceName(),
                                     type.Identifier.ValueText,
                                     members.OfType<MethodDeclarationSyntax>()
                                            .Where(m => m.Modifiers.Any(SyntaxKind.StaticKeyword) && IsValidPatchMethodName(m.Identifier.ValueText))
                                            .ToImmutableArray(),
                                     members.OfType<FieldDeclarationSyntax>()
                                            .Where(m => m.Modifiers.Any(SyntaxKind.StaticKeyword) && m.GetReturnTypeName() == "MethodInfo" && IsValidTargetMethodFieldName(m.GetName()))
                                            .ToImmutableArray());
    }

    internal record NitroxHarmonyType(string NameSpace, string TypeName, ImmutableArray<MethodDeclarationSyntax> HarmonyPatchMethods, ImmutableArray<FieldDeclarationSyntax> MethodInfoFields)
    {
        internal sealed class NitroxHarmonyTypeEqualityComparer : IEqualityComparer<NitroxHarmonyType>
        {
            public static IEqualityComparer<NitroxHarmonyType> Instance { get; } = new NitroxHarmonyTypeEqualityComparer();

            public bool Equals(NitroxHarmonyType x, NitroxHarmonyType y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.NameSpace, y.NameSpace) &&
                       string.Equals(x.TypeName, y.TypeName) &&
                       x.HarmonyPatchMethods.SequenceEqual(y.HarmonyPatchMethods, HarmonyMethodEqualityComparer.Instance) &&
                       x.MethodInfoFields.SequenceEqual(y.MethodInfoFields, MethodInfoFieldEqualityComparer.Instance);
            }

            public int GetHashCode(NitroxHarmonyType obj)
            {
                unchecked
                {
                    int hashCode = obj.NameSpace != null ? obj.NameSpace.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (obj.TypeName != null ? obj.TypeName.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private sealed class HarmonyMethodEqualityComparer : IEqualityComparer<MethodDeclarationSyntax>
        {
            public static IEqualityComparer<MethodDeclarationSyntax> Instance { get; } = new HarmonyMethodEqualityComparer();

            public bool Equals(MethodDeclarationSyntax x, MethodDeclarationSyntax y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Identifier.ValueText, y.Identifier.ValueText);
            }

            public int GetHashCode(MethodDeclarationSyntax obj)
            {
                return obj.Identifier.ValueText.GetHashCode();
            }
        }

        private sealed class MethodInfoFieldEqualityComparer : IEqualityComparer<FieldDeclarationSyntax>
        {
            public static IEqualityComparer<FieldDeclarationSyntax> Instance { get; } = new MethodInfoFieldEqualityComparer();

            public bool Equals(FieldDeclarationSyntax x, FieldDeclarationSyntax y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.ToString() == y.ToString();
            }

            public int GetHashCode(FieldDeclarationSyntax obj)
            {
                return obj.ToString().GetHashCode();
            }
        }
    }
}
