using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record WriteLoggerMiddleware : ILoggerMiddleware
{
    public delegate void WriteCall(ref ILoggerMiddleware.Context context);

    public required WriteCall Writer { get; init; } = (ref ILoggerMiddleware.Context _) => { };

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        Writer(ref context);
        next(ref context);
    }
}
