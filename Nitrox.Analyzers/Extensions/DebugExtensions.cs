using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nitrox.Analyzers.Extensions;

public static class DebugExtensions
{
    private static readonly string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); 
    private static readonly ConcurrentQueue<(DiagnosticAnalyzer analyzer, string message)> logQueue = new();
    private static readonly object logLocker = new();

    /// <summary>
    ///     Can be used to test analyzers.
    /// </summary>
    [Conditional("DEBUG")]
    public static void Log(this DiagnosticAnalyzer analyzer, string message)
    {
        logQueue.Enqueue((analyzer, message));
        Task.Run(() =>
        {
            while (!logQueue.IsEmpty)
            {
                if (!logQueue.TryDequeue(out (DiagnosticAnalyzer analyzer, string message) pair))
                {
                    continue;
                }
                
                lock (logLocker)
                {
                    File.AppendAllText(Path.Combine(desktopPath, $"{pair.analyzer.GetType().Name}.log"), pair.message + Environment.NewLine);
                }
            }
        });
    }
}
