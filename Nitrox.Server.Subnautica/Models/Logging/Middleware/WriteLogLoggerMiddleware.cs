using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record WriteLogLoggerMiddleware : ILoggerMiddleware
{
    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        context.Entry.ToString(context.Writer);
        next(ref context);
    }
}
