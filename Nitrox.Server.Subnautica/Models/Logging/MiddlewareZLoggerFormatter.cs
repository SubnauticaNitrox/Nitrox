using System.Buffers;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging;

/// <summary>
///     A ZLogger formatter which delegates the log writing task to its configurable middleware.
/// </summary>
internal abstract class MiddlewareZLoggerFormatter : IZLoggerFormatter
{
    public ILoggerMiddleware[] Middleware { get; protected set; } = [];

    /// <remarks>
    ///     ZLogger always writes newlines if true. So we set this to false as to only write new line character when something
    ///     is being logged.
    /// </remarks>
    public bool WithLineBreak => false;

    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        ILoggerMiddleware.Context context = new()
        {
            Writer = writer,
            Entry = entry,
            Middleware = Middleware
        };
        ExecuteNext(ref context);
    }

    private void ExecuteNext(ref ILoggerMiddleware.Context context)
    {
        if (GetNextMiddleware(ref context) is not { } middleware)
        {
            return;
        }

        middleware.ExecuteLogMiddleware(ref context, ExecuteNext);
    }

    private ILoggerMiddleware? GetNextMiddleware(ref ILoggerMiddleware.Context context)
    {
        if (context.Middleware.Length < 1)
        {
            return null;
        }
        if (context.Cursor >= context.Middleware.Length)
        {
            return null;
        }
        return context.Middleware[context.Cursor++];
    }
}
