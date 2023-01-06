using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nitrox.Analyzers.Diagnostics;

/// <summary>
///     Tests that requested localization keys exist in the English localization file.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalizationAnalyzer : DiagnosticAnalyzer
{
    public const string INVALID_LOCALIZATION_KEY_DIAGNOSTIC_ID = $"{nameof(LocalizationAnalyzer)}001";

    private const string NITROX_LOCALIZATION_PREFIX = "Nitrox_";
    private static readonly string relativePathFromSolutionDirToEnglishLanguageFile = Path.Combine("Nitrox.Assets.Subnautica", "LanguageFiles", "en.json");
    private static readonly Regex localizationParseRegex = new(@"^\s*""([^""]+)""\s*:\s*""([^""]+)""", RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly DiagnosticDescriptor invalidLocalizationKeyRule = new(INVALID_LOCALIZATION_KEY_DIAGNOSTIC_ID,
                                                                                  "Tests localization usages are valid",
                                                                                  "Localization key '{0}' does not exist in '{1}'",
                                                                                  "Usage",
                                                                                  DiagnosticSeverity.Warning,
                                                                                  true,
                                                                                  "Tests that requested localization keys exist in the English localization file");

    /// <summary>
    ///     Gets the list of rules of supported diagnostics.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(invalidLocalizationKeyRule);

    /// <summary>
    ///     Initializes the analyzer by registering on symbol occurrence in the targeted code.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            IMethodSymbol languageGetMethodSymbol = startContext.Compilation.GetTypesByMetadataName("Language").FirstOrDefault(a => a.ContainingAssembly.Name.Equals("Assembly-Csharp", StringComparison.OrdinalIgnoreCase))?.GetMembers("Get").FirstOrDefault(m => m.Kind == SymbolKind.Method) as IMethodSymbol;
            if (languageGetMethodSymbol == null)
            {
                return;
            }

            startContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.projectdir", out string projectDir);
            if (LocalizationHelper.Load(projectDir))
            {
                startContext.RegisterSyntaxNodeAction(c => AnalyzeStringNode(c, languageGetMethodSymbol), SyntaxKind.StringLiteralExpression);
            }
        });
    }

    /// <summary>
    ///     Analyzes string literals in code that are passed as argument to 'Language.main.Get'.
    /// </summary>
    private void AnalyzeStringNode(SyntaxNodeAnalysisContext context, IMethodSymbol languageGetMethodSymbol)
    {
        LiteralExpressionSyntax expression = (LiteralExpressionSyntax)context.Node;
        if (expression.Parent is not ArgumentSyntax argument)
        {
            return;
        }
        if (argument.Parent is not { Parent: InvocationExpressionSyntax invocation })
        {
            return;
        }
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return;
        }
        if (!SymbolEqualityComparer.Default.Equals(method, languageGetMethodSymbol))
        {
            return;
        }
        // Ignore language call for non-nitrox keys.
        string stringValue = expression.Token.ValueText;
        if (!stringValue.StartsWith(NITROX_LOCALIZATION_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        if (LocalizationHelper.ContainsKey(stringValue))
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(invalidLocalizationKeyRule, context.Node.GetLocation(), stringValue, LocalizationHelper.FileName));
    }

    /// <summary>
    ///     Wrapper API for synchronized access to the English localization file.
    /// </summary>
    private static class LocalizationHelper
    {
        private static readonly object locker = new();
        private static string EnglishLocalizationFileName { get; set; } = "";
        private static ImmutableDictionary<string, string> EnglishLocalization { get; set; } = ImmutableDictionary<string, string>.Empty;

        public static bool IsEmpty
        {
            get
            {
                lock (locker)
                {
                    return EnglishLocalization.IsEmpty;
                }
            }
        }

        public static string FileName
        {
            get
            {
                lock (locker)
                {
                    return EnglishLocalizationFileName;
                }
            }
        }

        public static bool ContainsKey(string key)
        {
            lock (locker)
            {
                return EnglishLocalization.ContainsKey(key);
            }
        }

        public static bool Load(string projectDir)
        {
            if (string.IsNullOrWhiteSpace(projectDir))
            {
                return false;
            }
            string solutionDir = Directory.GetParent(projectDir)?.Parent?.FullName;
            if (!Directory.Exists(solutionDir))
            {
                return false;
            }

            string enJson;
            lock (locker)
            {
                EnglishLocalizationFileName = Path.Combine(solutionDir, relativePathFromSolutionDirToEnglishLanguageFile);
                if (!File.Exists(EnglishLocalizationFileName))
                {
                    return false;
                }

                enJson = File.ReadAllText(EnglishLocalizationFileName);
            }
            // Parse localization JSON to dictionary for lookup.
            Dictionary<string, string> keyValue = new();
            foreach (Match match in localizationParseRegex.Matches(enJson))
            {
                keyValue.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
            lock (locker)
            {
                EnglishLocalization = keyValue.ToImmutableDictionary();
            }
            return true;
        }
    }
}
