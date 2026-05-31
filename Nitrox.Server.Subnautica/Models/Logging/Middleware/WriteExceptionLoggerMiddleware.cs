using System.Buffers;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record WriteExceptionLoggerMiddleware : ILoggerMiddleware
{
    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (context.Entry.LogInfo.Exception is { } exception)
        {
            // exception message
            context.Writer.Write("\n"u8);
            context.Writer.Write(exception.ToString());
        }

        next(ref context);
    }
}
