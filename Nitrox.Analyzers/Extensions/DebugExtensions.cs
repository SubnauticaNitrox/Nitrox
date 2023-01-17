using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Nitrox.Analyzers.Extensions;

public static class DebugExtensions
{
    private static readonly string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private static readonly ConcurrentQueue<(object source, string message)> logQueue = new();
    private static readonly object logLocker = new();

    /// <summary>
    ///     Can be used to test analyzers.
    /// </summary>
    [Conditional("DEBUG")]
    public static void Log(this object analyzer, string message)
    {
        if (analyzer == null)
        {
            return;
        }

        logQueue.Enqueue((analyzer, message));
        Task.Run(() =>
        {
            while (!logQueue.IsEmpty)
            {
                if (!logQueue.TryDequeue(out (object source, string message) pair))
                {
                    continue;
                }

                lock (logLocker)
                {
                    File.AppendAllText(Path.Combine(desktopPath, $"{pair.source.GetType().Name}.log"), pair.message + Environment.NewLine);
                }
            }
        });
    }
}
