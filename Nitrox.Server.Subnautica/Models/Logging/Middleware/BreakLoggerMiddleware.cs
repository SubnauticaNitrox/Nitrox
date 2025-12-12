using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record BreakLoggerMiddleware : ILoggerMiddleware
{
    public required Func<ILoggerMiddleware.Context, bool> BreakCondition { get; init; }

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        if (BreakCondition(context))
        {
            return;
        }
        next(ref context);
    }
}
