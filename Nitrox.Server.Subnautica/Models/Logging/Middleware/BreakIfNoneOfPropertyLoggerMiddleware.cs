using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record BreakIfNoneOfPropertyLoggerMiddleware : ILoggerMiddleware
{
    public HashSet<Type> Properties { get; set; } = [];

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        if (!context.Entry.ContainsAnyProperty(Properties))
        {
            return;
        }
        next(ref context);
    }
}
