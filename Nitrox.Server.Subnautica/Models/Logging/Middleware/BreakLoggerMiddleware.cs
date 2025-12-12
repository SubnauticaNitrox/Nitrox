using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record BreakLoggerMiddleware : ILoggerMiddleware
{
    public delegate bool BreakCall(ref ILoggerMiddleware.Context context);

    public required BreakCall BreakCondition { get; init; }

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        if (BreakCondition(ref context))
        {
            return;
        }
        next(ref context);
    }
}
