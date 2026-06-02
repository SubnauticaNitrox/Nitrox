using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed class ConditionalGroupLoggerMiddleware : ILoggerMiddleware
{
    public required Func<ILoggerMiddleware.Context, bool> Condition { get; init; }
    public required ILoggerMiddleware[] TrueGroup { get; init; }
    public ILoggerMiddleware[] FalseGroup { get; init; } = [];

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        ILoggerMiddleware[] selectedGroup = Condition(context) ? TrueGroup : FalseGroup;
        if (selectedGroup.Length > 0)
        {
            ILoggerMiddleware[] originalMiddleware = context.Middleware;
            int originalCursor = context.Cursor;
            context.Middleware = selectedGroup;
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
        }
        next(ref context);
    }
}
