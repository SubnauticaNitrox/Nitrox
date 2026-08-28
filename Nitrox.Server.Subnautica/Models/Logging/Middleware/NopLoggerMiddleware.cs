using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

/// <summary>
///     Does nothing except calling the next middleware. Use this when a middleware should be stubbed out.
/// </summary>
internal sealed record NopLoggerMiddleware : ILoggerMiddleware
{
    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        next(ref context);
    }
}
