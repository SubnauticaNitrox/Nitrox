using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Nitrox.Analyzers.Extensions;

namespace Nitrox.Analyzers.Generators;

[Generator(LanguageNames.CSharp)]
internal sealed class UnitySkippedObjectLifetimeHelperGenerator : IIncrementalGenerator
{
    public const string FIX_FUNCTION_NAME = "AliveOrNull";

    [SuppressMessage("ReSharper", "SuggestVarOrType_Elsewhere")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Setup compilation pipeline for assemblies that use UnityEngine.
        var compilationPipeline = context.CompilationProvider.Select((c, _) => c.HasType("UnityEngine.CoreModule", "UnityEngine.Object") != null);

        // Register the pipeline into the compiler.
        context.RegisterSourceOutput(compilationPipeline, static (context, hasUnityObject) => Execute(context, hasUnityObject));
    }

    private static void Execute(SourceProductionContext sourceProductionContext, bool hasUnityObject)
    {
        if (!hasUnityObject)
        {
            return;
        }

        sourceProductionContext.AddSource($"Extension{FIX_FUNCTION_NAME}.g.cs", $$"""
        using System;

        internal static class Extension{{FIX_FUNCTION_NAME}}
        {
            /// <summary>
            ///     Returns null if Unity has marked this object as dead.
            /// </summary>
            /// <param name="obj">Unity <see cref="UnityEngine.Object" /> to check if alive.</param>
            /// <typeparam name="TObject">Type of Unity object that can be marked as either alive or dead.</typeparam>
            /// <returns>The <see cref="UnityEngine.Object" /> if alive or null if dead.</returns>
            internal static TObject {{FIX_FUNCTION_NAME}}<TObject>(this TObject obj) where TObject : UnityEngine.Object
            {
                if (obj)
                {
                    return obj;
                }

                return null;
            }
        }
        """);
    }
}
