using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed class ConditionalGroupLoggerMiddleware : ILoggerMiddleware
{
    public required Func<ILoggerMiddleware.Context, bool> Condition { get; init; } = _ => true;
    public required ILoggerMiddleware[] Group { get; init; } = [];


    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        if (Condition(context))
        {
            ILoggerMiddleware[] originalMiddleware = context.Middleware;
            int originalCursor = context.Cursor;
            context.Middleware = Group;
            context.Cursor = 0;
            try
            {
                ExecuteNext(ref context);
            }
            finally
            {
                context.Middleware = originalMiddleware;
                context.Cursor = originalCursor;
            }
        }
        next(ref context);
    }

    private static void ExecuteNext(ref ILoggerMiddleware.Context context)
    {
        if (GetNextMiddleware(ref context) is not { } middleware)
        {
            return;
        }

        middleware.ExecuteLogMiddleware(ref context, ExecuteNext);
    }

    private static ILoggerMiddleware? GetNextMiddleware(ref ILoggerMiddleware.Context context)
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
