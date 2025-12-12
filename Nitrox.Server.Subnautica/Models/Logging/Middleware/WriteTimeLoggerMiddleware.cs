using System.Buffers;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record WriteTimeLoggerMiddleware : ILoggerMiddleware
{
    public required string Format { get; init; } = "";

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        DateTimeOffset datetime = context.Entry.LogInfo.Timestamp.Local;
        Span<byte> dateTimeDestination = context.Writer.GetSpan();
        if (!datetime.TryFormat(dateTimeDestination, out int written, Format))
        {
            datetime.TryFormat(dateTimeDestination, out written);
        }
        context.Writer.Write(dateTimeDestination[..written]);

        next(ref context);
    }
}
