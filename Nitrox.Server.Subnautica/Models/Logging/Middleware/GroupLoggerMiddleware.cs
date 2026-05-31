using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed class GroupLoggerMiddleware : ILoggerMiddleware
{
    public required ILoggerMiddleware[] Group { get; init; } = [];

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        ILoggerMiddleware[] originalMiddleware = context.Middleware;
        int originalCursor = context.Cursor;
        context.Middleware = Group;
        context.Cursor = 0;
        try
        {
            ILoggerMiddleware.ExecuteNext(ref context);
        }
        finally
        {
            context.Middleware = originalMiddleware;
            context.Cursor = originalCursor;
        }
        next(ref context);
    }
}
