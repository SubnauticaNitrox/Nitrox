using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nitrox.Analyzers;

public static class DebugExtensions
{
    /// <summary>
    ///     Can be used to test analyzers.
    /// </summary>
    [Conditional("DEBUG")]
    public static void Log(this DiagnosticAnalyzer analyzer, string message) => File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{analyzer.GetType().Name}.log"), message + Environment.NewLine);
}
